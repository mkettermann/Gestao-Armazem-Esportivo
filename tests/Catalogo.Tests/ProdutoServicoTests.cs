using Catalogo.Application.DTOs;
using Catalogo.Application.Servicos;
using Catalogo.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Catalogo.Tests;

public class ProdutoServicoTests
{
    private readonly Mock<IProdutoRepositorio> _repositorioMock = new();
    private readonly ProdutoServico _servico;

    public ProdutoServicoTests()
    {
        _servico = new ProdutoServico(_repositorioMock.Object);
    }

    [Fact]
    public async Task CriarAsync_DeveRetornarErro_QuandoPrecoZero()
    {
        var dto = new CriarProdutoDto { nome = "Produto", descricao = "Desc", preco = 0 };

        var resultado = await _servico.criarAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarErro_QuandoProdutoNaoEncontrado()
    {
        _repositorioMock.Setup(r => r.obterPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Catalogo.Domain.Entidades.Produto?)null);

        var resultado = await _servico.obterPorIdAsync(Guid.NewGuid());

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarErro_QuandoPrecoNegativo()
    {
        var produto = Catalogo.Domain.Factories.ProdutoFactory.criar("Produto", "Desc", 10.00m);
        _repositorioMock.Setup(r => r.obterPorIdAsync(produto.id, default))
            .ReturnsAsync(produto);

        var dto = new AtualizarProdutoDto { nome = "Produto", descricao = "Desc", preco = -5 };
        var resultado = await _servico.atualizarAsync(produto.id, dto);

        resultado.foiSucesso.Should().BeFalse();
    }
}
