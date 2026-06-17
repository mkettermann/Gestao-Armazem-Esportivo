using System.Diagnostics;

namespace Shared.Mensageria;

/// <summary>
/// Fonte de rastreamento (tracing) compartilhada pela camada de mensageria.
/// Deve ser registrada no OpenTelemetry de cada serviço via <c>.AddSource(Telemetria.NomeFonte)</c>
/// para que os spans de publicação e consumo apareçam no mesmo trace distribuído.
/// </summary>
public static class Telemetria
{
    public const string NomeFonte = "Gestao.Mensageria";

    public static readonly ActivitySource Fonte = new(NomeFonte);
}
