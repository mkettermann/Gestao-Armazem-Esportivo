using Estoque.Application.Mensageria;
using Estoque.Infrastructure.Persistencia.Idempotencia;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Persistencia.Repositorios;

public class EventoProcessadoRepositorio : IEventoProcessadoRepositorio
{
    private readonly EstoqueDbContext _contexto;

    public EventoProcessadoRepositorio(EstoqueDbContext contexto) => _contexto = contexto;

    public async Task<RegistroEventoProcessado?> obterAsync(Guid idEvento, CancellationToken ct = default)
    {
        var registro = await _contexto.EventosProcessados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.idEvento == idEvento, ct);

        return registro is null
            ? null
            : new RegistroEventoProcessado(registro.idEvento, registro.rejeitado, registro.motivo);
    }

    public async Task registrarAsync(Guid idEvento, bool rejeitado, string? motivo, CancellationToken ct = default)
    {
        await _contexto.EventosProcessados.AddAsync(new EventoProcessado(idEvento, rejeitado, motivo), ct);
    }
}
