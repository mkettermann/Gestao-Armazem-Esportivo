using FluentValidation;
using Pedidos.Application.DTOs;

namespace Pedidos.Application.Validadores;

public sealed class CriarPedidoDtoValidador : AbstractValidator<CriarPedidoDto>
{
    public CriarPedidoDtoValidador()
    {
        RuleFor(p => p.documentoCliente)
            .NotEmpty().WithMessage("O documento do cliente é obrigatório.");

        RuleFor(p => p.nomeVendedor)
            .NotEmpty().WithMessage("O nome do vendedor é obrigatório.");

        RuleFor(p => p.itens)
            .NotEmpty().WithMessage("O pedido deve conter ao menos um item.");

        RuleForEach(p => p.itens).ChildRules(item =>
        {
            item.RuleFor(i => i.produtoId)
                .NotEqual(Guid.Empty).WithMessage("O identificador do produto é inválido.");
            item.RuleFor(i => i.quantidade)
                .GreaterThan(0).WithMessage("A quantidade de cada item deve ser maior que zero.");
        });
    }
}
