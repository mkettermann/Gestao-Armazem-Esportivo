using Estoque.Domain.Entidades;

namespace Estoque.Domain.Factories;

public static class ItemEstoqueFactory
{
    public static ItemEstoque criar(Guid produtoId, int quantidadeInicial = 0)
        => new ItemEstoque(Guid.NewGuid(), produtoId, quantidadeInicial);
}
