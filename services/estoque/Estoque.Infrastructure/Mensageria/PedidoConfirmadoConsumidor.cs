using Estoque.Application.Servicos;
using Shared.Contratos.Eventos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Estoque.Infrastructure.Mensageria;

public class PedidoConfirmadoConsumidor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PedidoConfirmadoConsumidor> _logger;
    private readonly IConnectionFactory _connectionFactory;
    private IConnection? _conexao;
    private IChannel? _canal;

    public PedidoConfirmadoConsumidor(IServiceScopeFactory scopeFactory,
                                      IConnectionFactory connectionFactory,
                                      ILogger<PedidoConfirmadoConsumidor> logger)
    {
        _scopeFactory = scopeFactory;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        _conexao = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        _canal = await _conexao.CreateChannelAsync(cancellationToken: stoppingToken);

        await _canal.ExchangeDeclareAsync("pedidos.dlx", ExchangeType.Direct,
            durable: true, cancellationToken: stoppingToken);

        await _canal.QueueDeclareAsync(
            queue: "estoque.pedido-confirmado",
            durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "pedidos.dlx",
                ["x-dead-letter-routing-key"] = "estoque.pedido-confirmado.dlq"
            },
            cancellationToken: stoppingToken);

        await _canal.QueueDeclareAsync(
            queue: "estoque.pedido-confirmado.dlq",
            durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        await _canal.ExchangeDeclareAsync("pedidos.events", ExchangeType.Direct,
            durable: true, autoDelete: false, cancellationToken: stoppingToken);

        await _canal.QueueBindAsync(
            queue: "estoque.pedido-confirmado",
            exchange: "pedidos.events",
            routingKey: "pedidos.pedido.confirmado",
            cancellationToken: stoppingToken);

        await _canal.BasicQosAsync(0, 1, false, stoppingToken);

        var consumidor = new AsyncEventingBasicConsumer(_canal);
        consumidor.ReceivedAsync += async (_, ea) =>
            await processarMensagemAsync(ea, stoppingToken);

        await _canal.BasicConsumeAsync("estoque.pedido-confirmado",
            autoAck: false, consumer: consumidor, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task processarMensagemAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<PedidoConfirmadoEvento>(ea.Body.Span);
            if (evento is null) { await _canal!.BasicAckAsync(ea.DeliveryTag, false, ct); return; }

            using var scope = _scopeFactory.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<EstoqueServico>();

            foreach (var item in evento.itens)
                await servico.baixarEstoquePorPedidoAsync(item.produtoId, item.quantidade, ct);

            await _canal!.BasicAckAsync(ea.DeliveryTag, false, ct);
            _logger.LogInformation("Estoque baixado para pedido {PedidoId}.", evento.pedidoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem de pedido confirmado. Redelivered={R}.",
                ea.Redelivered);
            await _canal!.BasicNackAsync(ea.DeliveryTag, false, requeue: !ea.Redelivered, ct);
        }
    }

    public override async Task StopAsync(CancellationToken ct)
    {
        if (_canal is not null) await _canal.DisposeAsync();
        if (_conexao is not null) await _conexao.DisposeAsync();
        await base.StopAsync(ct);
    }
}
