namespace Estoque.Infrastructure.Persistencia.Idempotencia;

/// <summary>
/// Registro de idempotência: marca um evento (por <see cref="idEvento"/>) como já processado e
/// guarda o resultado da baixa. Evita reaplicar a baixa em caso de reentrega da mensagem e permite
/// reemitir a mesma resposta na saga (re-drive) sem efeitos colaterais.
/// </summary>
public class EventoProcessado
{
    public Guid idEvento { get; private set; }
    public bool rejeitado { get; private set; }
    public string? motivo { get; private set; }
    public DateTime processadoEm { get; private set; }

    private EventoProcessado() { }

    public EventoProcessado(Guid idEvento, bool rejeitado, string? motivo)
    {
        this.idEvento = idEvento;
        this.rejeitado = rejeitado;
        this.motivo = motivo;
        processadoEm = DateTime.UtcNow;
    }
}
