using Estoque.Domain.Entidades;
using Estoque.Domain.Interfaces;
using Estoque.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Persistencia.Repositorios;

public class ItemEstoqueRepositorio : IItemEstoqueRepositorio
{
    private readonly EstoqueDbContext _contexto;

    public ItemEstoqueRepositorio(EstoqueDbContext contexto) => _contexto = contexto;

    public async Task<ItemEstoque?> obterPorProdutoIdAsync(Guid produtoId, CancellationToken ct = default) =>
        await _contexto.ItensEstoque.FirstOrDefaultAsync(i => i.produtoId == produtoId, ct);

    public async Task adicionarAsync(ItemEstoque item, CancellationToken ct = default) =>
        await _contexto.ItensEstoque.AddAsync(item, ct);

    public async Task salvarAlteracoesAsync(CancellationToken ct = default) =>
        await _contexto.SaveChangesAsync(ct);
}

public class EntradaEstoqueRepositorio : IEntradaEstoqueRepositorio
{
    private readonly EstoqueDbContext _contexto;

    public EntradaEstoqueRepositorio(EstoqueDbContext contexto) => _contexto = contexto;

    public async Task adicionarAsync(EntradaEstoque entrada, CancellationToken ct = default) =>
        await _contexto.EntradasEstoque.AddAsync(entrada, ct);

    public async Task salvarAlteracoesAsync(CancellationToken ct = default) =>
        await _contexto.SaveChangesAsync(ct);
}
