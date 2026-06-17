using Estoque.Application.DTOs;
using Estoque.Application.Mensageria;
using Estoque.Application.Servicos;
using Estoque.Domain.Entidades;
using Estoque.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Estoque.Tests;

public class EstoqueServicoTests
{
    private readonly Mock<IItemEstoqueRepositorio> _itemMock = new();
    private readonly Mock<IEntradaEstoqueRepositorio> _entradaMock = new();
    private readonly Mock<IEventoProcessadoRepositorio> _eventoMock = new();
    private readonly EstoqueServico _servico;

    public EstoqueServicoTests()
    {
        _servico = new EstoqueServico(_itemMock.Object, _entradaMock.Object, _eventoMock.Object);
    }

    [Fact]
    public async Task AdicionarEstoqueAsync_DeveRetornarErro_QuandoNotaFiscalVazia()
    {
        var dto = new AdicionarEstoqueDto { numeroNotaFiscal = "", quantidade = 10 };

        var resultado = await _servico.adicionarEstoqueAsync(Guid.NewGuid(), dto);

        resultado.foiSucesso.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessarBaixaPedidoAsync_DeveAplicarBaixa_QuandoEstoqueSuficiente()
    {
        var produtoId = Guid.NewGuid();
        var item = new ItemEstoque(Guid.NewGuid(), produtoId, 10);
        _eventoMock.Setup(r => r.obterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegistroEventoProcessado?)null);
        _itemMock.Setup(r => r.obterPorProdutoIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var (rejeitado, _) = await _servico.processarBaixaPedidoAsync(
            Guid.NewGuid(), [new ItemBaixa(produtoId, 3)]);

        rejeitado.Should().BeFalse();
        item.quantidadeDisponivel.Should().Be(7);
        _eventoMock.Verify(r => r.registrarAsync(It.IsAny<Guid>(), false, null, It.IsAny<CancellationToken>()), Times.Once);
        _itemMock.Verify(r => r.salvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarBaixaPedidoAsync_DeveRejeitar_QuandoEstoqueInsuficiente()
    {
        var produtoId = Guid.NewGuid();
        var item = new ItemEstoque(Guid.NewGuid(), produtoId, 2);
        _eventoMock.Setup(r => r.obterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegistroEventoProcessado?)null);
        _itemMock.Setup(r => r.obterPorProdutoIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var (rejeitado, motivo) = await _servico.processarBaixaPedidoAsync(
            Guid.NewGuid(), [new ItemBaixa(produtoId, 10)]);

        rejeitado.Should().BeTrue();
        motivo.Should().Contain("insuficiente");
        item.quantidadeDisponivel.Should().Be(2); // não houve baixa parcial
        _eventoMock.Verify(r => r.registrarAsync(It.IsAny<Guid>(), true, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarBaixaPedidoAsync_DeveSerIdempotente_QuandoEventoJaProcessado()
    {
        var idEvento = Guid.NewGuid();
        _eventoMock.Setup(r => r.obterAsync(idEvento, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegistroEventoProcessado(idEvento, rejeitado: false, motivo: null));

        var (rejeitado, _) = await _servico.processarBaixaPedidoAsync(
            idEvento, [new ItemBaixa(Guid.NewGuid(), 5)]);

        rejeitado.Should().BeFalse();
        // Reentrega não pode reaplicar a baixa nem regravar idempotência.
        _itemMock.Verify(r => r.obterPorProdutoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _itemMock.Verify(r => r.salvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventoMock.Verify(r => r.registrarAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
