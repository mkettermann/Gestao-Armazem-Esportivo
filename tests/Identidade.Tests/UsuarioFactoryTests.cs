using FluentAssertions;
using Identidade.Domain.Enums;
using Identidade.Domain.Excecoes;
using Identidade.Domain.Factories;

namespace Identidade.Tests;

public class UsuarioFactoryTests
{
    [Fact]
    public void Criar_DeveFalhar_QuandoSenhaMenorQue6Caracteres()
    {
        var acao = () => UsuarioFactory.criar("Nome", "a@b.com", "123", TipoUsuario.Vendedor);
        acao.Should().Throw<DomainException>().WithMessage("*mínimo 6*");
    }

    [Fact]
    public void Criar_DeveArmazenarHash_EnaoSenhaEmTextoPlano()
    {
        var usuario = UsuarioFactory.criar("Nome", "a@b.com", "senha123", TipoUsuario.Administrador);

        usuario.senhaHash.Should().NotBe("senha123");
        usuario.senhaHash.Should().StartWith("$2"); // prefixo de hash BCrypt
        usuario.tipoUsuario.Should().Be(TipoUsuario.Administrador);
    }
}
