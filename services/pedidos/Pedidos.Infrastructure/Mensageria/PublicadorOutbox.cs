using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pedidos.Infrastructure.Persistencia;
using Pedidos.Infrastructure.Persistencia.Outbox;
using Shared.Mensageria;

namespace Pedidos.Infrastructure.Mensageria;

/// <summary>
/// Processa o Transactional Outbox: lê periodicamente as mensagens não publicadas e as envia ao
/// RabbitMQ. Em caso de falha no broker, as mensagens permanecem pendentes e são reenviadas no
/// próximo ciclo — garantindo entrega ao menos uma vez (consumidores são idempotentes).
/// </summary>
public class PublicadorOutbox : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventoPublicador _publicador;
    private readonly ILogger<PublicadorOutbox> _logger;

    public PublicadorOutbox(IServiceScopeFactory scopeFactory, IEventoPublicador publicador,
                            ILogger<PublicadorOutbox> logger)
    {
        _scopeFactory = scopeFactory;
        _publicador = publicador;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await publicarPendentesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar mensagens do outbox.");
            }

            try { await Task.Delay(Intervalo, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task publicarPendentesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();

        var pendentes = await contexto.MensagensOutbox
            .Where(m => m.processadoEm == null)
            .OrderBy(m => m.ocorridoEm)
            .Take(50)
            .ToListAsync(ct);

        if (pendentes.Count == 0) return;

        foreach (var mensagem in pendentes)
        {
            var corpo = Encoding.UTF8.GetBytes(mensagem.conteudo);
            await _publicador.publicarAsync(corpo, mensagem.exchange, mensagem.routingKey, ct);
            mensagem.marcarProcessado();
        }

        await contexto.SaveChangesAsync(ct);
    }
}
