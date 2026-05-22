using Microsoft.Extensions.Logging;
using Pedidos.Application.Servicos;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Pedidos.Infrastructure.Mensageria;

public class RabbitMqPublicador : IEventoPublicador
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqPublicador> _logger;

    public RabbitMqPublicador(IConnectionFactory connectionFactory,
                              ILogger<RabbitMqPublicador> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task publicarAsync<T>(T evento, string exchange, string routingKey,
                                       CancellationToken ct = default)
    {
        await using var conexao = await _connectionFactory.CreateConnectionAsync(ct);
        await using var canal = await conexao.CreateChannelAsync(cancellationToken: ct);

        await canal.ExchangeDeclareAsync(exchange, ExchangeType.Direct,
            durable: true, autoDelete: false, cancellationToken: ct);

        var corpo = JsonSerializer.SerializeToUtf8Bytes(evento);
        var props = new BasicProperties { Persistent = true };

        await canal.BasicPublishAsync(exchange, routingKey, true, props, corpo, ct);

        _logger.LogInformation("Evento {Tipo} publicado em {Exchange}/{RoutingKey}.",
            typeof(T).Name, exchange, routingKey);
    }
}
