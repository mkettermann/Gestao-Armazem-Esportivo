using Asp.Versioning;
using FluentValidation;
using Identidade.Application.DTOs;
using Identidade.Application.Servicos;
using Identidade.Application.Validadores;
using Identidade.Domain.Excecoes;
using Identidade.Domain.Interfaces;
using Identidade.Infrastructure.Persistencia;
using Identidade.Infrastructure.Persistencia.Repositorios;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Shared.Contratos.Extensoes;
using Shared.Contratos.Filtros;
using Shared.Contratos.Respostas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.AddScoped<ValidacaoModeloFilter>();
builder.Services.AddScoped<IValidator<CadastrarUsuarioDto>, CadastrarUsuarioDtoValidador>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidador>();
builder.Services.AddControllers(opts =>
{
    opts.Filters.AddService<ValidacaoModeloFilter>();
    opts.Filters.AddService<IdempotencyFilter>();
})
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddApiVersioning(opcoes =>
{
    opcoes.DefaultApiVersion = new ApiVersion(1, 0);
    opcoes.AssumeDefaultVersionWhenUnspecified = true;
    opcoes.ReportApiVersions = true;
    opcoes.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identidade API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.adicionarAutenticacaoJwt(builder.Configuration);
builder.Services.adicionarCorsPadrao(builder.Configuration);

builder.Services.AddDbContext<IdentidadeDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Identidade")));

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<TokenServico>();
builder.Services.AddScoped<AutenticacaoServico>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentidadeDbContext>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Npgsql")
        .AddOtlpExporter(opts => opts.Endpoint = new Uri(
            builder.Configuration["Observabilidade:OtlpEndpoint"] ?? "http://localhost:4317")))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(opts => opts.Endpoint = new Uri(
            builder.Configuration["Observabilidade:OtlpEndpoint"] ?? "http://localhost:4317")));

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var (status, mensagem) = feature?.Error switch
    {
        DomainException ex => (400, ex.Message),
        UnauthorizedAccessException => (401, "Acesso não autorizado."),
        _ => (500, "Ocorreu um erro interno. Tente novamente mais tarde.")
    };

    if (status == 500 && feature?.Error is { } excecao)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("TratamentoExcecoes");
        logger.LogError(excecao, "Erro não tratado em {Metodo} {Caminho}.",
            context.Request.Method, context.Request.Path);
    }

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(RespostaApi.Erro(mensagem));
}));

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");
app.UseCors(CorsExtensoes.PoliticaPadrao);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IdentidadeDbContext>();
    db.Database.Migrate();
}

app.Run();

// Exposto para os testes de integração (WebApplicationFactory<Program>).
public partial class Program { }



