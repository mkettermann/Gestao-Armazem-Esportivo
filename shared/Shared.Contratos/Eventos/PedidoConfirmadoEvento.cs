namespace Shared.Contratos.Eventos;

public sealed class PedidoConfirmadoEvento
{
    public Guid idEvento { get; init; } = Guid.NewGuid();
    public Guid pedidoId { get; init; }
    public DateTime dataConfirmacao { get; init; }
    public IReadOnlyList<ItemPedidoEvento> itens { get; init; } = [];
}

public sealed class ItemPedidoEvento
{
    public Guid produtoId { get; init; }
    public int quantidade { get; init; }
}
