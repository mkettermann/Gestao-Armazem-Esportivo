using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Interfaces;
using Shared.Contratos.Eventos;
using Shared.Mensageria;

namespace Pedidos.Infrastructure.Mensageria;

/// <summary>
/// Etapa final da saga: consome as respostas do serviço de Estoque e transiciona o pedido.
/// Baixa confirmada → <c>confirmar()</c>; estoque insuficiente → <c>rejeitar()</c>.
/// É idempotente por estado: respostas repetidas para um pedido já finalizado são ignoradas.
/// </summary>
public class RespostaEstoqueConsumidor : ConsumidorRabbitMqBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RespostaEstoqueConsumidor(IConexaoRabbitMq conexao, IServiceScopeFactory scopeFactory,
                                     ILogger<RespostaEstoqueConsumidor> logger)
        : base(conexao, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string Fila => RotasMensageria.FilaPedidosRespostaEstoque;
    protected override string Exchange => RotasMensageria.ExchangeEstoque;
    protected override IReadOnlyCollection<string> RoutingKeys =>
        [RotasMensageria.RoutingEstoqueBaixado, RotasMensageria.RoutingEstoqueRejeitado];
    protected override string NomeAtividade => "pedidos.processar-resposta-estoque";

    protected override async Task processarAsync(string routingKey, ReadOnlyMemory<byte> corpo, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IPedidoRepositorio>();

        if (routingKey == RotasMensageria.RoutingEstoqueBaixado)
        {
            var evento = JsonSerializer.Deserialize<EstoqueBaixadoEvento>(corpo.Span)
                ?? throw new InvalidOperationException("Evento de baixa de estoque inválido.");

            var pedido = await repositorio.obterPorIdAsync(evento.pedidoId, ct);
            if (pedido is null || pedido.status != StatusPedido.Pendente) return;

            pedido.confirmar();
            await repositorio.salvarAlteracoesAsync(ct);
            _logger.LogInformation("Pedido {PedidoId} confirmado após baixa de estoque.", evento.pedidoId);
        }
        else if (routingKey == RotasMensageria.RoutingEstoqueRejeitado)
        {
            var evento = JsonSerializer.Deserialize<EstoqueRejeitadoEvento>(corpo.Span)
                ?? throw new InvalidOperationException("Evento de rejeição de estoque inválido.");

            var pedido = await repositorio.obterPorIdAsync(evento.pedidoId, ct);
            if (pedido is null || pedido.status != StatusPedido.Pendente) return;

            pedido.rejeitar(evento.motivo);
            await repositorio.salvarAlteracoesAsync(ct);
            _logger.LogWarning("Pedido {PedidoId} rejeitado por estoque: {Motivo}", evento.pedidoId, evento.motivo);
        }
    }
}
