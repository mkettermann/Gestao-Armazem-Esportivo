using System.Text.Json;
using Estoque.Application.Mensageria;
using Estoque.Application.Servicos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Contratos.Eventos;
using Shared.Mensageria;

namespace Estoque.Infrastructure.Mensageria;

/// <summary>
/// Consome o evento de pedido registrado e dá baixa no estoque de forma idempotente e transacional.
/// Em seguida responde à saga: <see cref="EstoqueBaixadoEvento"/> em caso de sucesso ou
/// <see cref="EstoqueRejeitadoEvento"/> quando não há estoque suficiente.
/// </summary>
public class PedidoRegistradoConsumidor : ConsumidorRabbitMqBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventoPublicador _publicador;

    public PedidoRegistradoConsumidor(IConexaoRabbitMq conexao, IServiceScopeFactory scopeFactory,
                                      IEventoPublicador publicador,
                                      ILogger<PedidoRegistradoConsumidor> logger)
        : base(conexao, logger)
    {
        _scopeFactory = scopeFactory;
        _publicador = publicador;
    }

    protected override string Fila => RotasMensageria.FilaEstoquePedidoRegistrado;
    protected override string Exchange => RotasMensageria.ExchangePedidos;
    protected override IReadOnlyCollection<string> RoutingKeys => [RotasMensageria.RoutingPedidoRegistrado];
    protected override string NomeAtividade => "estoque.processar-pedido-registrado";

    protected override async Task processarAsync(string routingKey, ReadOnlyMemory<byte> corpo, CancellationToken ct)
    {
        var evento = JsonSerializer.Deserialize<PedidoRegistradoEvento>(corpo.Span)
            ?? throw new InvalidOperationException("Evento de pedido registrado inválido.");

        using var scope = _scopeFactory.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<EstoqueServico>();

        var itens = evento.itens.Select(i => new ItemBaixa(i.produtoId, i.quantidade)).ToList();
        var (rejeitado, motivo) = await servico.processarBaixaPedidoAsync(evento.idEvento, itens, ct);

        if (rejeitado)
        {
            var resposta = new EstoqueRejeitadoEvento
            {
                pedidoId = evento.pedidoId,
                motivo = motivo ?? "Estoque insuficiente."
            };
            await _publicador.publicarAsync(JsonSerializer.SerializeToUtf8Bytes(resposta),
                RotasMensageria.ExchangeEstoque, RotasMensageria.RoutingEstoqueRejeitado, ct);
        }
        else
        {
            var resposta = new EstoqueBaixadoEvento { pedidoId = evento.pedidoId };
            await _publicador.publicarAsync(JsonSerializer.SerializeToUtf8Bytes(resposta),
                RotasMensageria.ExchangeEstoque, RotasMensageria.RoutingEstoqueBaixado, ct);
        }
    }
}
