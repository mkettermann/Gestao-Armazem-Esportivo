using System.Diagnostics;
using System.Text;

namespace Shared.Mensageria;

/// <summary>
/// Propaga o contexto de trace (padrão W3C Trace Context) através das mensagens RabbitMQ,
/// gravando e lendo o <c>traceparent</c>/<c>tracestate</c> nos cabeçalhos. Sem isso, o trace
/// quebra na fronteira assíncrona e o Jaeger exibe spans desconexos entre publicador e consumidor.
/// </summary>
public static class PropagacaoTrace
{
    public const string CabecalhoTraceParent = "traceparent";
    public const string CabecalhoTraceState = "tracestate";

    public static void injetar(Activity? atividade, IDictionary<string, object?> cabecalhos)
    {
        if (atividade is null) return;

        cabecalhos[CabecalhoTraceParent] = atividade.Id;
        if (!string.IsNullOrEmpty(atividade.TraceStateString))
            cabecalhos[CabecalhoTraceState] = atividade.TraceStateString;
    }

    public static (string? traceParent, string? traceState) extrair(IDictionary<string, object?>? cabecalhos)
    {
        if (cabecalhos is null) return (null, null);
        return (ler(cabecalhos, CabecalhoTraceParent), ler(cabecalhos, CabecalhoTraceState));
    }

    private static string? ler(IDictionary<string, object?> cabecalhos, string chave)
    {
        if (!cabecalhos.TryGetValue(chave, out var valor) || valor is null)
            return null;

        // O RabbitMQ entrega valores de cabeçalho string como byte[].
        return valor is byte[] bytes ? Encoding.UTF8.GetString(bytes) : valor.ToString();
    }
}
