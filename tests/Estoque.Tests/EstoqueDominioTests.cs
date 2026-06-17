using Estoque.Domain.Entidades;
using Estoque.Domain.Excecoes;
using Estoque.Domain.Factories;
using FluentAssertions;

namespace Estoque.Tests;

public class EstoqueDominioTests
{
    [Fact]
    public void AdicionarQuantidade_DeveSomarAoDisponivel()
    {
        var item = new ItemEstoque(Guid.NewGuid(), Guid.NewGuid(), 5);
        item.adicionarQuantidade(3);
        item.quantidadeDisponivel.Should().Be(8);
    }

    [Fact]
    public void BaixarQuantidade_DeveSubtrairDoDisponivel()
    {
        var item = new ItemEstoque(Guid.NewGuid(), Guid.NewGuid(), 5);
        item.baixarQuantidade(2);
        item.quantidadeDisponivel.Should().Be(3);
    }

    [Fact]
    public void BaixarQuantidade_DeveFalhar_QuandoInsuficiente()
    {
        var item = new ItemEstoque(Guid.NewGuid(), Guid.NewGuid(), 1);
        var acao = () => item.baixarQuantidade(5);
        acao.Should().Throw<DomainException>().WithMessage("*insuficiente*");
    }

    [Fact]
    public void EntradaEstoqueFactory_DeveFalhar_QuandoNotaFiscalVazia()
    {
        var acao = () => EntradaEstoqueFactory.criar(Guid.NewGuid(), 10, "");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void EntradaEstoqueFactory_DeveCriar_QuandoDadosValidos()
    {
        var entrada = EntradaEstoqueFactory.criar(Guid.NewGuid(), 10, "NF-123");
        entrada.numeroNotaFiscal.Should().Be("NF-123");
        entrada.quantidade.Should().Be(10);
    }
}
