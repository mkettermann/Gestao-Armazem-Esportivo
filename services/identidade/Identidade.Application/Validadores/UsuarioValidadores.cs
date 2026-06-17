using FluentValidation;
using Identidade.Application.DTOs;

namespace Identidade.Application.Validadores;

public sealed class CadastrarUsuarioDtoValidador : AbstractValidator<CadastrarUsuarioDto>
{
    public CadastrarUsuarioDtoValidador()
    {
        RuleFor(u => u.nome)
            .NotEmpty().WithMessage("O nome do usuário é obrigatório.")
            .MaximumLength(200);

        RuleFor(u => u.email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(u => u.senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

        RuleFor(u => u.tipoUsuario)
            .IsInEnum().WithMessage("O tipo de usuário é inválido.");
    }
}

public sealed class LoginDtoValidador : AbstractValidator<LoginDto>
{
    public LoginDtoValidador()
    {
        RuleFor(l => l.email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(l => l.senha)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
