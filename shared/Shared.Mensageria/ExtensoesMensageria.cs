using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Shared.Mensageria;

public static class ExtensoesMensageria
{
    /// <summary>
    /// Registra a infraestrutura de mensageria RabbitMQ: fábrica de conexão, conexão persistente
    /// compartilhada (<see cref="IConexaoRabbitMq"/>) e publicador de eventos (<see cref="IEventoPublicador"/>).
    /// </summary>
    public static IServiceCollection adicionarMensageriaRabbitMq(
        this IServiceCollection services, IConfiguration configuracao)
    {
        services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
        {
            HostName = configuracao["RabbitMQ:Host"] ?? "localhost",
            UserName = configuracao["RabbitMQ:Usuario"] ?? "guest",
            Password = configuracao["RabbitMQ:Senha"] ?? "guest"
        });
        services.AddSingleton<IConexaoRabbitMq, ConexaoRabbitMq>();
        services.AddSingleton<IEventoPublicador, RabbitMqPublicador>();
        return services;
    }
}
