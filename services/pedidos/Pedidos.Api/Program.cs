using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Pedidos.Api;
using Pedidos.Application.Clientes;
using Pedidos.Application.Servicos;
using Pedidos.Domain.Excecoes;
using Pedidos.Domain.Interfaces;
using Pedidos.Infrastructure.Mensageria;
using Pedidos.Infrastructure.Persistencia;
using Pedidos.Infrastructure.Persistencia.Repositorios;
using RabbitMQ.Client;
using Shared.Contratos.Extensoes;
using Shared.Contratos.Filtros;
using Shared.Contratos.Respostas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.AddControllers(opts => opts.Filters.AddService<IdempotencyFilter>());
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
builder.Services.AddDbContext<PedidosDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Pedidos")));
builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
        UserName = builder.Configuration["RabbitMQ:Usuario"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Senha"] ?? "guest"
    });
builder.Services.AddScoped<IEventoPublicador, RabbitMqPublicador>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AutenticacaoForwardHandler>();

var estoqueUrl = builder.Configuration["ServicosInternos:EstoqueUrl"]
    ?? throw new InvalidOperationException("Configuração 'ServicosInternos:EstoqueUrl' é obrigatória.");
var catalogoUrl = builder.Configuration["ServicosInternos:CatalogoUrl"]
    ?? throw new InvalidOperationException("Configuração 'ServicosInternos:CatalogoUrl' é obrigatória.");

builder.Services.AddHttpClient<EstoqueClienteHttp>(client =>
{
    client.BaseAddress = new Uri(estoqueUrl);
}).AddHttpMessageHandler<AutenticacaoForwardHandler>();
builder.Services.AddHttpClient<CatalogoClienteHttp>(client =>
{
    client.BaseAddress = new Uri(catalogoUrl);
}).AddHttpMessageHandler<AutenticacaoForwardHandler>();
builder.Services.AddScoped<PedidoServico>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PedidosDbContext>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
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
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(RespostaApi.Erro(mensagem));
}));

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
    db.Database.Migrate();
}

app.Run();



