using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contratos.Eventos;

namespace Shared.Mensageria;

/// <summary>
/// Base para consumidores RabbitMQ. Centraliza topologia (fila durável com dead-letter), QoS,
/// extração do contexto de trace, e a política de ACK/NACK: na primeira falha reenfileira; na
/// reincidência envia para a DLQ. As subclasses só implementam <see cref="processarAsync"/>.
/// </summary>
public abstract class ConsumidorRabbitMqBase : BackgroundService
{
    private readonly IConexaoRabbitMq _conexao;
    protected readonly ILogger _logger;
    private IChannel? _canal;

    protected ConsumidorRabbitMqBase(IConexaoRabbitMq conexao, ILogger logger)
    {
        _conexao = conexao;
        _logger = logger;
    }

    protected abstract string Fila { get; }
    protected abstract string Exchange { get; }
    protected abstract IReadOnlyCollection<string> RoutingKeys { get; }
    protected abstract string NomeAtividade { get; }

    /// <summary>Processa a mensagem. Lançar exceção sinaliza falha (NACK/DLQ).</summary>
    protected abstract Task processarAsync(string routingKey, ReadOnlyMemory<byte> corpo, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _canal = await criarCanalComEsperaAsync(stoppingToken);

        var dlq = $"{Fila}.dlq";
        await _canal.ExchangeDeclareAsync(RotasMensageria.ExchangeDlx, ExchangeType.Direct,
            durable: true, cancellationToken: stoppingToken);
        await _canal.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);
        await _canal.QueueBindAsync(dlq, RotasMensageria.ExchangeDlx, dlq, cancellationToken: stoppingToken);

        await _canal.QueueDeclareAsync(Fila, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = RotasMensageria.ExchangeDlx,
                ["x-dead-letter-routing-key"] = dlq
            },
            cancellationToken: stoppingToken);

        await _canal.ExchangeDeclareAsync(Exchange, ExchangeType.Direct,
            durable: true, autoDelete: false, cancellationToken: stoppingToken);
        foreach (var routingKey in RoutingKeys)
            await _canal.QueueBindAsync(Fila, Exchange, routingKey, cancellationToken: stoppingToken);

        await _canal.BasicQosAsync(0, 1, false, stoppingToken);

        var consumidor = new AsyncEventingBasicConsumer(_canal);
        consumidor.ReceivedAsync += (_, ea) => receberAsync(ea, stoppingToken);
        await _canal.BasicConsumeAsync(Fila, autoAck: false, consumer: consumidor, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<IChannel> criarCanalComEsperaAsync(CancellationToken ct)
    {
        while (true)
        {
            try
            {
                return await _conexao.criarCanalAsync(ct: ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "RabbitMQ indisponível para a fila {Fila}. Nova tentativa em 5s.", Fila);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }

    private async Task receberAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        var (traceParent, traceState) = PropagacaoTrace.extrair(ea.BasicProperties.Headers);
        using var atividade = Telemetria.Fonte.StartActivity(NomeAtividade, ActivityKind.Consumer, traceParent);
        if (atividade is not null && !string.IsNullOrEmpty(traceState))
            atividade.TraceStateString = traceState;

        try
        {
            await processarAsync(ea.RoutingKey, ea.Body, ct);
            await _canal!.BasicAckAsync(ea.DeliveryTag, false, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem da fila {Fila}. Redelivered={Redelivered}.",
                Fila, ea.Redelivered);
            await _canal!.BasicNackAsync(ea.DeliveryTag, false, requeue: !ea.Redelivered, ct);
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_canal is not null) await _canal.DisposeAsync();
        await base.StopAsync(ct);
    }
}
