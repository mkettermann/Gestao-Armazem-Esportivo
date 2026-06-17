using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Identidade.Application.Servicos;
using Identidade.Domain.Enums;
using Identidade.Domain.Factories;
using Moq;

namespace Identidade.Tests;

public class TokenServicoTests
{
    private static TokenServico criarServico()
    {
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["JWT:Chave"]).Returns("chave-de-teste-com-mais-de-32-caracteres-ok");
        configMock.Setup(c => c["JWT:Emissor"]).Returns("GestaoArmazem");
        configMock.Setup(c => c["JWT:Audiencia"]).Returns("GestaoArmazem");
        configMock.Setup(c => c["JWT:ExpiracaoEmHoras"]).Returns("8");
        return new TokenServico(configMock.Object);
    }

    [Fact]
    public void GerarToken_DeveEmitirJwtComPapelEExpiracao()
    {
        var servico = criarServico();
        var usuario = UsuarioFactory.criar("Admin", "admin@email.com", "senha123", TipoUsuario.Administrador);

        var resposta = servico.gerarToken(usuario);

        resposta.token.Should().NotBeNullOrWhiteSpace();
        resposta.tipoUsuario.Should().Be("Administrador");
        resposta.expiracao.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(resposta.token);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Administrador");
    }
}
