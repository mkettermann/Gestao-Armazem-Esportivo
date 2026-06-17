using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Mensageria;

public interface IEventoPublicador
{
    /// <summary>
    /// Publica um corpo já serializado em um exchange direto durável, com mensagem persistente,
    /// confirmação do broker (publisher confirms) e propagação do contexto de trace.
    /// </summary>
    Task publicarAsync(ReadOnlyMemory<byte> corpo, string exchange, string routingKey,
                       CancellationToken ct = default);
}

public sealed class RabbitMqPublicador : IEventoPublicador
{
    private readonly IConexaoRabbitMq _conexao;
    private readonly ILogger<RabbitMqPublicador> _logger;

    public RabbitMqPublicador(IConexaoRabbitMq conexao, ILogger<RabbitMqPublicador> logger)
    {
        _conexao = conexao;
        _logger = logger;
    }

    public async Task publicarAsync(ReadOnlyMemory<byte> corpo, string exchange, string routingKey,
                                    CancellationToken ct = default)
    {
        using var atividade = Telemetria.Fonte.StartActivity(
            $"publicar {exchange}/{routingKey}", ActivityKind.Producer);

        await using var canal = await _conexao.criarCanalAsync(confirmacaoPublicacao: true, ct);

        await canal.ExchangeDeclareAsync(exchange, ExchangeType.Direct,
            durable: true, autoDelete: false, cancellationToken: ct);

        var cabecalhos = new Dictionary<string, object?>();
        PropagacaoTrace.injetar(Activity.Current, cabecalhos);

        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Headers = cabecalhos
        };

        await canal.BasicPublishAsync(exchange, routingKey, mandatory: true,
            basicProperties: propriedades, body: corpo, cancellationToken: ct);

        _logger.LogInformation("Evento publicado em {Exchange}/{RoutingKey}.", exchange, routingKey);
    }
}
