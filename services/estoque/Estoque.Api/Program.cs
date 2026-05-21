using Estoque.Application.Servicos;
using Estoque.Domain.Excecoes;
using Estoque.Domain.Interfaces;
using Estoque.Infrastructure.Mensageria;
using Estoque.Infrastructure.Persistencia;
using Estoque.Infrastructure.Persistencia.Repositorios;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
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
    c.SwaggerDoc("v1", new() { Title = "Estoque API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new() { { new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

builder.Services.adicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AddDbContext<EstoqueDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Estoque")));
builder.Services.AddScoped<IItemEstoqueRepositorio, ItemEstoqueRepositorio>();
builder.Services.AddScoped<IEntradaEstoqueRepositorio, EntradaEstoqueRepositorio>();
builder.Services.AddScoped<EstoqueServico>();
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
        UserName = builder.Configuration["RabbitMQ:Usuario"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Senha"] ?? "guest"
    });
builder.Services.AddHostedService<PedidoConfirmadoConsumidor>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EstoqueDbContext>();

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
    var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
    db.Database.Migrate();
}

app.Run();



