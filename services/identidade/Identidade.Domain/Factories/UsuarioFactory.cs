using BCrypt.Net;
using Identidade.Domain.Entidades;
using Identidade.Domain.Enums;
using Identidade.Domain.Excecoes;
using Identidade.Domain.ValueObjects;

namespace Identidade.Domain.Factories;

public static class UsuarioFactory
{
    public static Usuario criar(string nome, string email,
                                string senhaTextoPlano, TipoUsuario tipoUsuario)
    {
        if (string.IsNullOrWhiteSpace(senhaTextoPlano) || senhaTextoPlano.Length < 6)
            throw new DomainException("A senha deve ter no mínimo 6 caracteres.");

        var emailVo = Email.criar(email);
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaTextoPlano);
        return Usuario.criar(Guid.NewGuid(), nome, emailVo, senhaHash, tipoUsuario);
    }
}
