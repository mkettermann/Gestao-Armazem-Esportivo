using RabbitMQ.Client;

namespace Shared.Mensageria;

/// <summary>
/// Abstrai uma conexão RabbitMQ persistente e reutilizável. Abrir uma conexão por mensagem é caro;
/// aqui a conexão é criada sob demanda e compartilhada, criando-se apenas canais (baratos) por operação.
/// </summary>
public interface IConexaoRabbitMq : IAsyncDisposable
{
    Task<IChannel> criarCanalAsync(bool confirmacaoPublicacao = false, CancellationToken ct = default);
}

public sealed class ConexaoRabbitMq : IConexaoRabbitMq
{
    private readonly IConnectionFactory _fabrica;
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private IConnection? _conexao;

    public ConexaoRabbitMq(IConnectionFactory fabrica) => _fabrica = fabrica;

    public async Task<IChannel> criarCanalAsync(bool confirmacaoPublicacao = false, CancellationToken ct = default)
    {
        var conexao = await obterConexaoAsync(ct);

        var opcoes = confirmacaoPublicacao
            ? new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)
            : null;

        return await conexao.CreateChannelAsync(opcoes, ct);
    }

    private async Task<IConnection> obterConexaoAsync(CancellationToken ct)
    {
        if (_conexao is { IsOpen: true }) return _conexao;

        await _semaforo.WaitAsync(ct);
        try
        {
            if (_conexao is { IsOpen: true }) return _conexao;
            _conexao = await _fabrica.CreateConnectionAsync(ct);
            return _conexao;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexao is not null) await _conexao.DisposeAsync();
        _semaforo.Dispose();
    }
}
