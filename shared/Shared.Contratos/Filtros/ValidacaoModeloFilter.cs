using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Contratos.Respostas;

namespace Shared.Contratos.Filtros;

/// <summary>
/// Executa, antes da ação, o validador FluentValidation correspondente a cada argumento (quando
/// registrado). Em caso de falha, curto-circuita com 400 no mesmo envelope <see cref="RespostaApi"/>
/// das demais respostas, mantendo a consistência do padrão facade.
/// </summary>
public sealed class ValidacaoModeloFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _provedor;

    public ValidacaoModeloFilter(IServiceProvider provedor) => _provedor = provedor;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null) continue;

            var tipoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (_provedor.GetService(tipoValidador) is not IValidator validador) continue;

            var resultado = await validador.ValidateAsync(new ValidationContext<object>(argumento));
            if (!resultado.IsValid)
            {
                var mensagem = string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage));
                context.Result = new BadRequestObjectResult(RespostaApi.Erro(mensagem));
                return;
            }
        }

        await next();
    }
}
