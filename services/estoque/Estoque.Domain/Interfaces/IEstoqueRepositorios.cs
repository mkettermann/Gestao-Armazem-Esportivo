using Estoque.Domain.Entidades;

namespace Estoque.Domain.Interfaces;

public interface IItemEstoqueRepositorio
{
    Task<ItemEstoque?> obterPorProdutoIdAsync(Guid produtoId, CancellationToken ct = default);
    Task adicionarAsync(ItemEstoque item, CancellationToken ct = default);
    Task salvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IEntradaEstoqueRepositorio
{
    Task adicionarAsync(EntradaEstoque entrada, CancellationToken ct = default);
    Task salvarAlteracoesAsync(CancellationToken ct = default);
}
