using Identidade.Domain.Entidades;

namespace Identidade.Domain.Interfaces;

public interface IUsuarioRepositorio
{
    Task<Usuario?> obterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> obterPorEmailAsync(string email, CancellationToken ct = default);
    Task adicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task salvarAlteracoesAsync(CancellationToken ct = default);
}
