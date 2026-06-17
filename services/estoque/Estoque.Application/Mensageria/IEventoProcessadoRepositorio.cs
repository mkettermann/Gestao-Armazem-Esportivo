namespace Estoque.Application.Mensageria;

/// <summary>Item a ter baixa de estoque dentro de um pedido.</summary>
public readonly record struct ItemBaixa(Guid produtoId, int quantidade);

/// <summary>Resultado persistido do processamento idempotente de um evento.</summary>
public sealed record RegistroEventoProcessado(Guid idEvento, bool rejeitado, string? motivo);

/// <summary>
/// Controle de idempotência do consumidor de pedidos. As gravações participam da mesma transação
/// da baixa de estoque (mesmo DbContext), tornando "baixar" e "marcar como processado" atômicos.
/// </summary>
public interface IEventoProcessadoRepositorio
{
    Task<RegistroEventoProcessado?> obterAsync(Guid idEvento, CancellationToken ct = default);
    Task registrarAsync(Guid idEvento, bool rejeitado, string? motivo, CancellationToken ct = default);
}
