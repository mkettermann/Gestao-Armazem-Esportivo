using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Contratos.Respostas;
using System.Text;
using System.Text.Json;

namespace Shared.Contratos.Extensoes;

public static class AutenticacaoExtensoes
{
    public static IServiceCollection adicionarAutenticacaoJwt(
        this IServiceCollection servicos,
        IConfiguration config)
    {
        var chaveTexto = config["JWT:Chave"]
            ?? throw new InvalidOperationException("Configuração 'JWT:Chave' é obrigatória.");
        if (chaveTexto.Length < 32)
            throw new InvalidOperationException(
                "Configuração 'JWT:Chave' deve ter ao menos 32 caracteres para garantir a segurança da assinatura.");
        var chave = Encoding.UTF8.GetBytes(chaveTexto);

        servicos
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JWT:Emissor"],
                    ValidAudience = config["JWT:Audiencia"],
                    IssuerSigningKey = new SymmetricSecurityKey(chave),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var bytes = System.Text.Encoding.UTF8.GetBytes(
                            JsonSerializer.Serialize(RespostaApi.Erro("Token inválido ou não informado.")));
                        await context.Response.Body.WriteAsync(bytes);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var bytes = System.Text.Encoding.UTF8.GetBytes(
                            JsonSerializer.Serialize(RespostaApi.Erro("Você não tem permissão para acessar este recurso.")));
                        await context.Response.Body.WriteAsync(bytes);
                    }
                };
            });

        servicos.AddAuthorization();
        return servicos;
    }
}
