using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entidades;
using Pedidos.Domain.Interfaces;
using Pedidos.Infrastructure.Persistencia;

namespace Pedidos.Infrastructure.Persistencia.Repositorios;

public class PedidoRepositorio : IPedidoRepositorio
{
    private readonly PedidosDbContext _contexto;

    public PedidoRepositorio(PedidosDbContext contexto) => _contexto = contexto;

    public async Task<Pedido?> obterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _contexto.Pedidos
            .Include(p => p.itens)
            .FirstOrDefaultAsync(p => p.id == id, ct);

    public async Task adicionarAsync(Pedido pedido, CancellationToken ct = default) =>
        await _contexto.Pedidos.AddAsync(pedido, ct);

    public async Task salvarAlteracoesAsync(CancellationToken ct = default) =>
        await _contexto.SaveChangesAsync(ct);
}
