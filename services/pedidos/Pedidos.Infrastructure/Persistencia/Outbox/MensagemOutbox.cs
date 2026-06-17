namespace Pedidos.Infrastructure.Persistencia.Outbox;

/// <summary>
/// Mensagem do padrão Transactional Outbox. É gravada na MESMA transação do pedido, garantindo que
/// "registrar o pedido" e "agendar a publicação do evento" sejam atômicos (elimina o dual-write).
/// Um processo em segundo plano publica as pendentes e marca <see cref="processadoEm"/>.
/// </summary>
public class MensagemOutbox
{
    public Guid id { get; private set; }
    public string tipo { get; private set; } = string.Empty;
    public string conteudo { get; private set; } = string.Empty;
    public string exchange { get; private set; } = string.Empty;
    public string routingKey { get; private set; } = string.Empty;
    public DateTime ocorridoEm { get; private set; }
    public DateTime? processadoEm { get; private set; }

    private MensagemOutbox() { }

    public MensagemOutbox(string tipo, string conteudo, string exchange, string routingKey)
    {
        id = Guid.NewGuid();
        this.tipo = tipo;
        this.conteudo = conteudo;
        this.exchange = exchange;
        this.routingKey = routingKey;
        ocorridoEm = DateTime.UtcNow;
    }

    public void marcarProcessado() => processadoEm = DateTime.UtcNow;
}
