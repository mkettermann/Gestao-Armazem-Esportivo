using System.Text.RegularExpressions;
using Identidade.Domain.Excecoes;

namespace Identidade.Domain.ValueObjects;

/// <summary>
/// Value object que encapsula e valida um endereço de e-mail.
/// Garante que nenhum e-mail inválido transite pelo domínio.
/// </summary>
public sealed record Email
{
    private static readonly Regex _regex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string valor { get; }

    private Email(string valor) => this.valor = valor;

    public static Email criar(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail é obrigatório.");

        var normalizado = email.ToLowerInvariant().Trim();

        if (!_regex.IsMatch(normalizado))
            throw new DomainException("O e-mail informado é inválido.");

        return new Email(normalizado);
    }

    public static implicit operator string(Email email) => email.valor;
    public override string ToString() => valor;
}
