namespace Shared.Contratos.Eventos;

/// <summary>
/// Emitido pelo serviço de Estoque quando não há estoque suficiente para um pedido (ex.: corrida
/// entre pedidos concorrentes). Faz o serviço de Pedidos transicionar o pedido para Rejeitado,
/// fechando a janela de venda além do disponível (etapa de compensação da saga).
/// </summary>
public sealed class EstoqueRejeitadoEvento
{
    public Guid idEvento { get; init; } = Guid.NewGuid();
    public Guid pedidoId { get; init; }
    public string motivo { get; init; } = string.Empty;
}
