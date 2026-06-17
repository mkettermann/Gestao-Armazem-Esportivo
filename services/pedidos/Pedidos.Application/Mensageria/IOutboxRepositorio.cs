namespace Pedidos.Application.Mensageria;

/// <summary>
/// Acrescenta um evento de integração ao outbox dentro da unidade de trabalho atual (mesmo
/// DbContext do pedido). A publicação efetiva no broker é feita posteriormente pelo processo de
/// outbox; aqui apenas registra-se a intenção, garantindo atomicidade com a persistência do pedido.
/// </summary>
public interface IOutboxRepositorio
{
    Task adicionarAsync<T>(T evento, string exchange, string routingKey, CancellationToken ct = default);
}
