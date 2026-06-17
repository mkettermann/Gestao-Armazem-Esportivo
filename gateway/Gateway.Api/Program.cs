var builder = WebApplication.CreateBuilder(args);

const string politicaCors = "PoliticaPadraoCors";
var origensCors = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? [];

// CORS no ponto de entrada público (gateway), consumido pela aplicação web cliente.
builder.Services.AddCors(opcoes => opcoes.AddPolicy(politicaCors, politica =>
{
    if (origensCors.Length > 0)
        politica.WithOrigins(origensCors).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    else
        politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors(politicaCors);
app.MapReverseProxy();
app.Run();
