using FluentAssertions;
using Moq;
using Pedidos.Application.Clientes;
using Pedidos.Application.DTOs;
using Pedidos.Application.Servicos;
using Pedidos.Domain.Entidades;
using Pedidos.Domain.Interfaces;
using Shared.Contratos.Resultados;

namespace Pedidos.Tests;

public class PedidoServicoTests
{
    private readonly Mock<IPedidoRepositorio> _repositorioMock = new();
    private readonly Mock<EstoqueClienteHttp> _estoqueClienteMock;
    private readonly Mock<IEventoPublicador> _publicadorMock = new();
    private readonly PedidoServico _servico;

    public PedidoServicoTests()
    {
        _estoqueClienteMock = new Mock<EstoqueClienteHttp>(
            new System.Net.Http.HttpClient());
        _servico = new PedidoServico(
            _repositorioMock.Object,
            _estoqueClienteMock.Object,
            _publicadorMock.Object);
    }

    [Fact]
    public async Task EmitirAsync_NaoDeveCriarPedido_QuandoEstoqueInsuficiente()
    {
        var produtoId = Guid.NewGuid();
        _estoqueClienteMock.Setup(c => c.obterQuantidadeDisponivel(produtoId, default))
            .ReturnsAsync(Resultado<int>.Sucesso(2));

        var dto = new CriarPedidoDto
        {
            documentoCliente = "123.456.789-00",
            nomeVendedor = "Vendedor Teste",
            itens = [new ItemPedidoDto { produtoId = produtoId, quantidade = 10 }]
        };

        var resultado = await _servico.emitirAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("insuficiente");
        _repositorioMock.Verify(r => r.adicionarAsync(It.IsAny<Pedido>(), default), Times.Never);
    }

    [Fact]
    public async Task EmitirAsync_DevePublicarEvento_QuandoSucesso()
    {
        var produtoId = Guid.NewGuid();
        _estoqueClienteMock.Setup(c => c.obterQuantidadeDisponivel(produtoId, default))
            .ReturnsAsync(Resultado<int>.Sucesso(50));
        _repositorioMock.Setup(r => r.adicionarAsync(It.IsAny<Pedido>(), default))
            .Returns(Task.CompletedTask);
        _repositorioMock.Setup(r => r.salvarAlteracoesAsync(default))
            .Returns(Task.CompletedTask);
        _publicadorMock.Setup(p => p.publicarAsync(
            It.IsAny<Shared.Contratos.Eventos.PedidoConfirmadoEvento>(),
            "pedidos.events", "pedidos.pedido.confirmado", default))
            .Returns(Task.CompletedTask);

        var dto = new CriarPedidoDto
        {
            documentoCliente = "123.456.789-00",
            nomeVendedor = "Vendedor Teste",
            itens = [new ItemPedidoDto { produtoId = produtoId, quantidade = 5 }]
        };

        var resultado = await _servico.emitirAsync(dto);

        resultado.foiSucesso.Should().BeTrue();
        _publicadorMock.Verify(p => p.publicarAsync(
            It.IsAny<Shared.Contratos.Eventos.PedidoConfirmadoEvento>(),
            "pedidos.events", "pedidos.pedido.confirmado", default), Times.Once);
    }

    [Fact]
    public async Task EmitirAsync_DeveRetornarErro_QuandoPedidoSemItens()
    {
        var dto = new CriarPedidoDto
        {
            documentoCliente = "123.456.789-00",
            nomeVendedor = "Vendedor Teste",
            itens = []
        };

        var resultado = await _servico.emitirAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("item");
    }
}
