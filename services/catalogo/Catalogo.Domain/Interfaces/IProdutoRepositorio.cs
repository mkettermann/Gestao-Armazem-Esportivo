using Catalogo.Domain.Entidades;

namespace Catalogo.Domain.Interfaces;

public interface IProdutoRepositorio
{
    Task<Produto?> obterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Produto> itens, int total)> listarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task adicionarAsync(Produto produto, CancellationToken ct = default);
    Task salvarAlteracoesAsync(CancellationToken ct = default);
}
