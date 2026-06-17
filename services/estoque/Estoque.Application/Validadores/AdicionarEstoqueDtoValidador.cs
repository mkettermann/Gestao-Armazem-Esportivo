using Estoque.Application.DTOs;
using FluentValidation;

namespace Estoque.Application.Validadores;

public sealed class AdicionarEstoqueDtoValidador : AbstractValidator<AdicionarEstoqueDto>
{
    public AdicionarEstoqueDtoValidador()
    {
        RuleFor(e => e.quantidade)
            .GreaterThan(0).WithMessage("A quantidade adquirida deve ser maior que zero.");

        RuleFor(e => e.numeroNotaFiscal)
            .NotEmpty().WithMessage("O número da nota fiscal é obrigatório.")
            .MaximumLength(100);
    }
}
