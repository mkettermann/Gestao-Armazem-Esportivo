using Identidade.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Integracao.Tests;

/// <summary>
/// Sobe a Identidade.Api em processo (ambiente "Testing": sem migrations Npgsql) e troca o banco por
/// EF InMemory, permitindo testar o pipeline HTTP real (validação, envelope, autenticação) sem Docker.
/// </summary>
public class FabricaIdentidadeApi : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT:Chave"] = "chave-de-teste-com-mais-de-32-caracteres-ok",
                ["JWT:Emissor"] = "GestaoArmazem",
                ["JWT:Audiencia"] = "GestaoArmazem",
                ["JWT:ExpiracaoEmHoras"] = "8",
                ["ConnectionStrings:Identidade"] = "Host=localhost;Database=fake;Username=x;Password=y"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove TODO o registro do provedor relacional (DbContextOptions, IDbContextOptionsConfiguration
            // do EF 9, e o próprio contexto) e substitui por EF InMemory.
            var remover = services.Where(d =>
                d.ServiceType == typeof(IdentidadeDbContext) ||
                d.ServiceType.FullName?.Contains("DbContextOptions") == true).ToList();
            foreach (var descritor in remover) services.Remove(descritor);

            services.AddDbContext<IdentidadeDbContext>(o => o.UseInMemoryDatabase("identidade-integracao"));
        });
    }
}
