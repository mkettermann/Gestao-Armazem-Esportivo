using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Contratos.Extensoes;

/// <summary>
/// Política de CORS compartilhada. A API é consumida por uma aplicação web cliente, portanto o CORS
/// é necessário. As origens permitidas vêm da configuração <c>Cors:Origens</c>; quando vazia (ex.:
/// ambiente de desenvolvimento) libera qualquer origem para facilitar testes.
/// </summary>
public static class CorsExtensoes
{
    public const string PoliticaPadrao = "PoliticaPadraoCors";

    public static IServiceCollection adicionarCorsPadrao(
        this IServiceCollection servicos, IConfiguration configuracao)
    {
        var origens = configuracao.GetSection("Cors:Origens").Get<string[]>() ?? [];

        servicos.AddCors(opcoes => opcoes.AddPolicy(PoliticaPadrao, politica =>
        {
            if (origens.Length > 0)
                politica.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            else
                politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }));

        return servicos;
    }
}
