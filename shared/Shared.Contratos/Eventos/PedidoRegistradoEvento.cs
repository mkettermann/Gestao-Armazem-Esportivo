namespace Shared.Contratos.Eventos;

/// <summary>
/// Emitido pelo serviço de Pedidos quando um pedido é registrado (status Pendente) e solicita a
/// baixa de estoque. O <see cref="idEvento"/> é a chave de idempotência usada pelo consumidor para
/// evitar baixa em duplicidade em caso de reentrega da mensagem.
/// </summary>
public sealed class PedidoRegistradoEvento
{
    public Guid idEvento { get; init; } = Guid.NewGuid();
    public Guid pedidoId { get; init; }
    public DateTime dataRegistro { get; init; }
    public IReadOnlyList<ItemPedidoEvento> itens { get; init; } = [];
}

public sealed class ItemPedidoEvento
{
    public Guid produtoId { get; init; }
    public int quantidade { get; init; }
}
