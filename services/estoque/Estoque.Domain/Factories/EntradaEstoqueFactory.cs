using Estoque.Domain.Entidades;

namespace Estoque.Domain.Factories;

public static class EntradaEstoqueFactory
{
    public static EntradaEstoque criar(Guid produtoId, int quantidade, string numeroNotaFiscal)
        => new EntradaEstoque(Guid.NewGuid(), produtoId, quantidade, numeroNotaFiscal);
}
