using Estoque.Application.DTOs;
using Estoque.Application.Servicos;
using Estoque.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Estoque.Tests;

public class EstoqueServicoTests
{
    private readonly Mock<IItemEstoqueRepositorio> _itemMock = new();
    private readonly Mock<IEntradaEstoqueRepositorio> _entradaMock = new();
    private readonly EstoqueServico _servico;

    public EstoqueServicoTests()
    {
        _servico = new EstoqueServico(_itemMock.Object, _entradaMock.Object);
    }

    [Fact]
    public async Task AdicionarEstoqueAsync_DeveRetornarErro_QuandoNotaFiscalVazia()
    {
        var dto = new AdicionarEstoqueDto { numeroNotaFiscal = "", quantidade = 10 };

        var resultado = await _servico.adicionarEstoqueAsync(Guid.NewGuid(), dto);

        resultado.foiSucesso.Should().BeFalse();
    }

    [Fact]
    public async Task BaixarEstoquePorPedidoAsync_DeveLancarExcecao_QuandoEstoqueNaoEncontrado()
    {
        var produtoId = Guid.NewGuid();

        _itemMock.Setup(r => r.obterPorProdutoIdAsync(produtoId, default))
            .ReturnsAsync((Estoque.Domain.Entidades.ItemEstoque?)null);

        var acao = async () => await _servico.baixarEstoquePorPedidoAsync(produtoId, 5);

        await acao.Should().ThrowAsync<Exception>();
    }
}
