using Catalogo.Domain.Entidades;
using Catalogo.Domain.Interfaces;
using Catalogo.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Persistencia.Repositorios;

public class ProdutoRepositorio : IProdutoRepositorio
{
    private readonly CatalogoDbContext _contexto;

    public ProdutoRepositorio(CatalogoDbContext contexto) => _contexto = contexto;

    public async Task<Produto?> obterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _contexto.Produtos
            .FirstOrDefaultAsync(p => p.id == id && p.ativo, ct);

    public async Task<(IEnumerable<Produto> itens, int total)> listarAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var query = _contexto.Produtos.Where(p => p.ativo);
        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderBy(p => p.nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);
        return (itens, total);
    }

    public async Task adicionarAsync(Produto produto, CancellationToken ct = default) =>
        await _contexto.Produtos.AddAsync(produto, ct);

    public async Task salvarAlteracoesAsync(CancellationToken ct = default) =>
        await _contexto.SaveChangesAsync(ct);

    public async Task<bool> existeComNomeAsync(string nome, Guid? excluirId, CancellationToken ct = default) =>
        await _contexto.Produtos.AnyAsync(
            p => p.ativo
                 && (excluirId == null || p.id != excluirId)
                 && p.nome.ToLower() == nome.ToLower().Trim(),
            ct);
}
