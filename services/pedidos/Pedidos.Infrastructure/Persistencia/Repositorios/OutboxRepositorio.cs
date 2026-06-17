using System.Text.Json;
using Pedidos.Application.Mensageria;
using Pedidos.Infrastructure.Persistencia.Outbox;

namespace Pedidos.Infrastructure.Persistencia.Repositorios;

public class OutboxRepositorio : IOutboxRepositorio
{
    private readonly PedidosDbContext _contexto;

    public OutboxRepositorio(PedidosDbContext contexto) => _contexto = contexto;

    public async Task adicionarAsync<T>(T evento, string exchange, string routingKey,
                                        CancellationToken ct = default)
    {
        var conteudo = JsonSerializer.Serialize(evento);
        var mensagem = new MensagemOutbox(typeof(T).Name, conteudo, exchange, routingKey);
        await _contexto.MensagensOutbox.AddAsync(mensagem, ct);
    }
}
