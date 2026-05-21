using Identidade.Domain.Entidades;
using Identidade.Domain.Interfaces;
using Identidade.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Identidade.Infrastructure.Persistencia.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly IdentidadeDbContext _contexto;

    public UsuarioRepositorio(IdentidadeDbContext contexto) => _contexto = contexto;

    public async Task<Usuario?> obterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _contexto.Usuarios.FirstOrDefaultAsync(u => u.id == id, ct);

    public async Task<Usuario?> obterPorEmailAsync(string email, CancellationToken ct = default) =>
        await _contexto.Usuarios
            .FirstOrDefaultAsync(u => u.email == email.ToLowerInvariant().Trim(), ct);

    public async Task adicionarAsync(Usuario usuario, CancellationToken ct = default) =>
        await _contexto.Usuarios.AddAsync(usuario, ct);

    public async Task salvarAlteracoesAsync(CancellationToken ct = default) =>
        await _contexto.SaveChangesAsync(ct);
}
