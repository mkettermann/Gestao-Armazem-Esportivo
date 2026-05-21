using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shared.Contratos.Respostas;
using System.Text.Json;

namespace Shared.Contratos.Filtros;

/// <summary>
/// Filtro de idempotência para endpoints de mutação (POST, PUT, PATCH, DELETE).
/// O cliente deve enviar o header <c>Idempotency-Key</c> com um UUID válido para ativar a garantia.
/// Requisições repetidas com a mesma chave recebem a resposta original armazenada em cache (24 h).
/// </summary>
public sealed class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IMemoryCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public IdempotencyFilter(IMemoryCache cache, IOptions<JsonOptions> jsonOptions)
    {
        _cache = cache;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        var chave = context.HttpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(chave))
        {
            await next();
            return;
        }

        if (!Guid.TryParse(chave, out _))
        {
            context.Result = new BadRequestObjectResult(
                RespostaApi.Erro("O header 'Idempotency-Key' deve conter um UUID válido."));
            return;
        }

        var cacheKey = $"idmp:{chave}";

        if (_cache.TryGetValue(cacheKey, out IdempotencyRegistro? registro))
        {
            context.HttpContext.Response.Headers["X-Idempotent-Replayed"] = "true";
            context.Result = new ContentResult
            {
                StatusCode = registro!.StatusCode,
                Content = registro.Corpo,
                ContentType = "application/json; charset=utf-8"
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult { Value: not null } objectResult
            && executedContext.Exception is null)
        {
            var json = JsonSerializer.Serialize(objectResult.Value, _jsonOptions);
            var novoRegistro = new IdempotencyRegistro(
                objectResult.StatusCode ?? context.HttpContext.Response.StatusCode,
                json);

            _cache.Set(cacheKey, novoRegistro, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
        }
    }
}

internal sealed record IdempotencyRegistro(int StatusCode, string Corpo);
