using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Asp.Versioning;
using FluentValidation;
using Pedidos.Api;
using Pedidos.Application.Clientes;
using Pedidos.Application.DTOs;
using Pedidos.Application.Mensageria;
using Pedidos.Application.Servicos;
using Pedidos.Application.Validadores;
using Pedidos.Domain.Excecoes;
using Pedidos.Domain.Interfaces;
using Pedidos.Infrastructure.Mensageria;
using Pedidos.Infrastructure.Persistencia;
using Pedidos.Infrastructure.Persistencia.Repositorios;
using Shared.Contratos.Extensoes;
using Shared.Contratos.Filtros;
using Shared.Contratos.Respostas;
using Shared.Mensageria;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.AddScoped<ValidacaoModeloFilter>();
builder.Services.AddScoped<IValidator<CriarPedidoDto>, CriarPedidoDtoValidador>();
builder.Services.AddControllers(opts =>
{
    opts.Filters.AddService<ValidacaoModeloFilter>();
    opts.Filters.AddService<IdempotencyFilter>();
});
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
    c.SwaggerDoc("v1", new() { Title = "Pedidos API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new() { { new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

builder.Services.adicionarAutenticacaoJwt(builder.Configuration);
builder.Services.adicionarCorsPadrao(builder.Configuration);
builder.Services.AddDbContext<PedidosDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Pedidos")));
builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
builder.Services.AddScoped<IOutboxRepositorio, OutboxRepositorio>();
builder.Services.adicionarMensageriaRabbitMq(builder.Configuration);
builder.Services.AddHostedService<PublicadorOutbox>();
builder.Services.AddHostedService<RespostaEstoqueConsumidor>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AutenticacaoForwardHandler>();

var estoqueUrl = builder.Configuration["ServicosInternos:EstoqueUrl"]
    ?? throw new InvalidOperationException("Configuração 'ServicosInternos:EstoqueUrl' é obrigatória.");
var catalogoUrl = builder.Configuration["ServicosInternos:CatalogoUrl"]
    ?? throw new InvalidOperationException("Configuração 'ServicosInternos:CatalogoUrl' é obrigatória.");

// Clientes HTTP internos com resiliência (timeout, retry com backoff e circuit breaker) para que
// uma indisponibilidade momentânea de Catálogo/Estoque não derrube a emissão do pedido.
builder.Services.AddHttpClient<EstoqueClienteHttp>(client =>
{
    client.BaseAddress = new Uri(estoqueUrl);
}).AddHttpMessageHandler<AutenticacaoForwardHandler>()
  .AddStandardResilienceHandler();
builder.Services.AddHttpClient<CatalogoClienteHttp>(client =>
{
    client.BaseAddress = new Uri(catalogoUrl);
}).AddHttpMessageHandler<AutenticacaoForwardHandler>()
  .AddStandardResilienceHandler();
builder.Services.AddScoped<PedidoServico>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PedidosDbContext>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(Telemetria.NomeFonte)
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
    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
    db.Database.Migrate();
}

app.Run();



