using FluentAssertions;
using Moq;
using Pedidos.Application.Clientes;
using Pedidos.Application.DTOs;
using Pedidos.Application.Mensageria;
using Pedidos.Application.Servicos;
using Pedidos.Domain.Entidades;
using Pedidos.Domain.Interfaces;
using Shared.Contratos.Eventos;
using Shared.Contratos.Resultados;

namespace Pedidos.Tests;

public class PedidoServicoTests
{
    private readonly Mock<IPedidoRepositorio> _repositorioMock = new();
    private readonly Mock<EstoqueClienteHttp> _estoqueClienteMock;
    private readonly Mock<CatalogoClienteHttp> _catalogoClienteMock;
    private readonly Mock<IOutboxRepositorio> _outboxMock = new();
    private readonly PedidoServico _servico;

    public PedidoServicoTests()
    {
        _estoqueClienteMock = new Mock<EstoqueClienteHttp>(new System.Net.Http.HttpClient());
        _catalogoClienteMock = new Mock<CatalogoClienteHttp>(new System.Net.Http.HttpClient());
        _servico = new PedidoServico(
            _repositorioMock.Object,
            _estoqueClienteMock.Object,
            _catalogoClienteMock.Object,
            _outboxMock.Object);
    }

    [Fact]
    public async Task EmitirAsync_NaoDeveCriarPedido_QuandoEstoqueInsuficiente()
    {
        var produtoId = Guid.NewGuid();
        _catalogoClienteMock.Setup(c => c.obterProdutoAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resultado<ProdutoExternoDto>.Sucesso(
                new ProdutoExternoDto { nome = "Produto Teste", preco = 10m }));
        _estoqueClienteMock.Setup(c => c.obterQuantidadeDisponivel(produtoId, It.IsAny<CancellationToken>()))
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
        _repositorioMock.Verify(r => r.adicionarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxMock.Verify(o => o.adicionarAsync(It.IsAny<PedidoRegistradoEvento>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EmitirAsync_DeveRegistrarPedidoPendenteEEnfileirarOutbox_QuandoSucesso()
    {
        var produtoId = Guid.NewGuid();
        _catalogoClienteMock.Setup(c => c.obterProdutoAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resultado<ProdutoExternoDto>.Sucesso(
                new ProdutoExternoDto { nome = "Produto Teste", preco = 99.9m }));
        _estoqueClienteMock.Setup(c => c.obterQuantidadeDisponivel(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Resultado<int>.Sucesso(50));

        var dto = new CriarPedidoDto
        {
            documentoCliente = "123.456.789-00",
            nomeVendedor = "Vendedor Teste",
            itens = [new ItemPedidoDto { produtoId = produtoId, quantidade = 5 }]
        };

        var resultado = await _servico.emitirAsync(dto);

        resultado.foiSucesso.Should().BeTrue();
        // O pedido NÃO é confirmado de imediato: só após a baixa de estoque (saga).
        resultado.valor!.status.Should().Be("Pendente");
        // Evento gravado no outbox (mesma transação) em vez de publicado diretamente (sem dual-write).
        _outboxMock.Verify(o => o.adicionarAsync(
            It.IsAny<PedidoRegistradoEvento>(),
            RotasMensageria.ExchangePedidos,
            RotasMensageria.RoutingPedidoRegistrado,
            It.IsAny<CancellationToken>()), Times.Once);
        _repositorioMock.Verify(r => r.adicionarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositorioMock.Verify(r => r.salvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
