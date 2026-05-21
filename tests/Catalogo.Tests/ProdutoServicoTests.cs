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
        _repositorioMock.Setup(r => r.existeComNomeAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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
        _repositorioMock.Setup(r => r.existeComNomeAsync(It.IsAny<string>(), produto.id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = new AtualizarProdutoDto { nome = "Produto", descricao = "Desc", preco = -5 };
        var resultado = await _servico.atualizarAsync(produto.id, dto);

        resultado.foiSucesso.Should().BeFalse();
    }

    // ── Unicidade de nome ──────────────────────────────────────────────────────

    [Fact]
    public async Task CriarAsync_DeveRetornarErro_QuandoNomeDuplicado()
    {
        _repositorioMock.Setup(r => r.existeComNomeAsync("Nike Air Max", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CriarProdutoDto { nome = "Nike Air Max", descricao = "Tênis", preco = 599m };
        var resultado = await _servico.criarAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("nome");
    }

    [Fact]
    public async Task CriarAsync_DeveRetornarErro_QuandoNomeDuplicadoCaseInsensitive()
    {
        // "NIKE AIR MAX" conflita com "Nike Air Max" porque o repositório usa LOWER()
        _repositorioMock.Setup(r => r.existeComNomeAsync("NIKE AIR MAX", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CriarProdutoDto { nome = "NIKE AIR MAX", descricao = "Tênis", preco = 599m };
        var resultado = await _servico.criarAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("nome");
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarErro_QuandoNomeDuplicadoEmOutroProduto()
    {
        var produto = Catalogo.Domain.Factories.ProdutoFactory.criar("Produto Original", "Desc", 100m);
        _repositorioMock.Setup(r => r.obterPorIdAsync(produto.id, default))
            .ReturnsAsync(produto);
        _repositorioMock.Setup(r => r.existeComNomeAsync("Produto Duplicado", produto.id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new AtualizarProdutoDto { nome = "Produto Duplicado", descricao = "Desc", preco = 100m };
        var resultado = await _servico.atualizarAsync(produto.id, dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("nome");
    }

    [Fact]
    public async Task AtualizarAsync_DevePermitir_QuandoNomePertenceAoMesmoProduto()
    {
        var produto = Catalogo.Domain.Factories.ProdutoFactory.criar("Produto Original", "Desc", 100m);
        _repositorioMock.Setup(r => r.obterPorIdAsync(produto.id, default))
            .ReturnsAsync(produto);
        // Mesmo nome, mas excluindo o próprio produto → sem conflito
        _repositorioMock.Setup(r => r.existeComNomeAsync("Produto Original", produto.id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositorioMock.Setup(r => r.salvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dto = new AtualizarProdutoDto { nome = "Produto Original", descricao = "Nova Descrição", preco = 150m };
        var resultado = await _servico.atualizarAsync(produto.id, dto);

        resultado.foiSucesso.Should().BeTrue();
    }
}

