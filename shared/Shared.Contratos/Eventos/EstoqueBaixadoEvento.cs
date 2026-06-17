namespace Shared.Contratos.Eventos;

/// <summary>
/// Emitido pelo serviço de Estoque após dar baixa com sucesso nos itens de um pedido. Faz o serviço
/// de Pedidos transicionar o pedido de Pendente para Confirmado (etapa de sucesso da saga).
/// </summary>
public sealed class EstoqueBaixadoEvento
{
    public Guid idEvento { get; init; } = Guid.NewGuid();
    public Guid pedidoId { get; init; }
}
