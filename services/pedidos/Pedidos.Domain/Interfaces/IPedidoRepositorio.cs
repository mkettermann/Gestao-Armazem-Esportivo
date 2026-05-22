using Pedidos.Domain.Entidades;

namespace Pedidos.Domain.Interfaces;

public interface IPedidoRepositorio
{
    Task<Pedido?> obterPorIdAsync(Guid id, CancellationToken ct = default);
    Task adicionarAsync(Pedido pedido, CancellationToken ct = default);
    Task salvarAlteracoesAsync(CancellationToken ct = default);
}
