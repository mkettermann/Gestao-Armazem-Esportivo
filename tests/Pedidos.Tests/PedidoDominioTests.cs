using FluentAssertions;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Excecoes;
using Pedidos.Domain.Factories;
using Pedidos.Domain.ValueObjects;

namespace Pedidos.Tests;

public class PedidoDominioTests
{
    private static Pedidos.Domain.Entidades.Pedido criarComItem()
    {
        var pedido = PedidoFactory.criar("123.456.789-00", "Vendedor");
        pedido.adicionarItem(Guid.NewGuid(), "Produto", 10m, 2);
        return pedido;
    }

    [Fact]
    public void NovoPedido_DeveNascerPendente()
        => criarComItem().status.Should().Be(StatusPedido.Pendente);

    [Fact]
    public void Confirmar_DeveTransicionarParaConfirmado()
    {
        var pedido = criarComItem();
        pedido.confirmar();
        pedido.status.Should().Be(StatusPedido.Confirmado);
        pedido.dataConfirmacao.Should().NotBeNull();
    }

    [Fact]
    public void Rejeitar_DeveTransicionarParaRejeitado_ComMotivo()
    {
        var pedido = criarComItem();
        pedido.rejeitar("Estoque insuficiente.");
        pedido.status.Should().Be(StatusPedido.Rejeitado);
        pedido.motivoRejeicao.Should().Be("Estoque insuficiente.");
    }

    [Fact]
    public void Rejeitar_DeveFalhar_QuandoPedidoNaoEstaPendente()
    {
        var pedido = criarComItem();
        pedido.confirmar();
        var acao = () => pedido.rejeitar("tarde demais");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void CalcularTotal_DeveSomarSubtotais()
    {
        var pedido = PedidoFactory.criar("123.456.789-00", "Vendedor");
        pedido.adicionarItem(Guid.NewGuid(), "A", 10m, 2);
        pedido.adicionarItem(Guid.NewGuid(), "B", 5m, 3);
        pedido.calcularTotal().Should().Be(35m);
    }

    [Fact]
    public void AdicionarItem_DeveFalhar_QuandoProdutoDuplicado()
    {
        var pedido = PedidoFactory.criar("123.456.789-00", "Vendedor");
        var produtoId = Guid.NewGuid();
        pedido.adicionarItem(produtoId, "A", 10m, 1);
        var acao = () => pedido.adicionarItem(produtoId, "A", 10m, 1);
        acao.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("123.456.789-00")]   // CPF
    [InlineData("12.345.678/0001-95")] // CNPJ
    public void DocumentoCliente_DeveAceitar_CpfOuCnpj(string documento)
        => DocumentoCliente.criar(documento).valor.Should().Be(documento);

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void DocumentoCliente_DeveFalhar_QuandoInvalido(string documento)
    {
        var acao = () => DocumentoCliente.criar(documento);
        acao.Should().Throw<DomainException>();
    }
}
