using Identidade.Domain.Enums;

namespace Identidade.Application.DTOs;

public sealed class CadastrarUsuarioDto
{
    public string nome { get; init; } = string.Empty;
    public string email { get; init; } = string.Empty;
    public string senha { get; init; } = string.Empty;
    public TipoUsuario tipoUsuario { get; init; }
}

public sealed class LoginDto
{
    public string email { get; init; } = string.Empty;
    public string senha { get; init; } = string.Empty;
}

public sealed class UsuarioRespostaDto
{
    public Guid id { get; init; }
    public string nome { get; init; } = string.Empty;
    public string email { get; init; } = string.Empty;
    public string tipoUsuario { get; init; } = string.Empty;
    public DateTime dataCadastro { get; init; }
}

public sealed class TokenRespostaDto
{
    public string token { get; init; } = string.Empty;
    public DateTime expiracao { get; init; }
    public string tipoUsuario { get; init; } = string.Empty;
}
