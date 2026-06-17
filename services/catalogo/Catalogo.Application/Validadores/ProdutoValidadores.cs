using Catalogo.Application.DTOs;
using FluentValidation;

namespace Catalogo.Application.Validadores;

public sealed class CriarProdutoDtoValidador : AbstractValidator<CriarProdutoDto>
{
    public CriarProdutoDtoValidador()
    {
        RuleFor(p => p.nome)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200);

        RuleFor(p => p.descricao)
            .NotEmpty().WithMessage("A descrição do produto é obrigatória.")
            .MaximumLength(2000);

        RuleFor(p => p.preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");
    }
}

public sealed class AtualizarProdutoDtoValidador : AbstractValidator<AtualizarProdutoDto>
{
    public AtualizarProdutoDtoValidador()
    {
        RuleFor(p => p.nome)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200);

        RuleFor(p => p.descricao)
            .NotEmpty().WithMessage("A descrição do produto é obrigatória.")
            .MaximumLength(2000);

        RuleFor(p => p.preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");
    }
}
