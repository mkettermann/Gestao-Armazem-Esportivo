using FluentAssertions;
using Identidade.Application.DTOs;
using Identidade.Application.Servicos;
using Identidade.Domain.Interfaces;
using Moq;

namespace Identidade.Tests;

public class AutenticacaoServicoTests
{
    private readonly Mock<IUsuarioRepositorio> _repositorioMock = new();
    private readonly Mock<TokenServico> _tokenServicoMock;
    private readonly AutenticacaoServico _servico;

    public AutenticacaoServicoTests()
    {
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["JWT:Chave"]).Returns("chave-de-teste-com-mais-de-32-caracteres-ok");
        configMock.Setup(c => c["JWT:Emissor"]).Returns("test");
        configMock.Setup(c => c["JWT:Audiencia"]).Returns("test");
        configMock.Setup(c => c["JWT:ExpiracaoEmHoras"]).Returns("8");
        _tokenServicoMock = new Mock<TokenServico>(configMock.Object);
        _servico = new AutenticacaoServico(_repositorioMock.Object, _tokenServicoMock.Object);
    }

    [Fact]
    public async Task CadastrarAsync_DeveRetornarErro_QuandoEmailJaExiste()
    {
        var dto = new CadastrarUsuarioDto
        {
            nome = "Teste",
            email = "teste@email.com",
            senha = "Senha123",
            tipoUsuario = Identidade.Domain.Enums.TipoUsuario.Vendedor
        };

        _repositorioMock.Setup(r => r.obterPorEmailAsync(dto.email, default))
            .ReturnsAsync(Identidade.Domain.Factories.UsuarioFactory.criar(
                "Existente", dto.email, "Senha123", Identidade.Domain.Enums.TipoUsuario.Vendedor));

        var resultado = await _servico.cadastrarAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("e-mail");
    }

    [Fact]
    public async Task CadastrarAsync_DeveRetornarErro_QuandoSenhaMenorQue6Caracteres()
    {
        var dto = new CadastrarUsuarioDto
        {
            nome = "Teste",
            email = "novo@email.com",
            senha = "abc",
            tipoUsuario = Identidade.Domain.Enums.TipoUsuario.Vendedor
        };

        _repositorioMock.Setup(r => r.obterPorEmailAsync(dto.email, default))
            .ReturnsAsync((Identidade.Domain.Entidades.Usuario?)null);

        var resultado = await _servico.cadastrarAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarErro_QuandoSenhaIncorreta()
    {
        var usuario = Identidade.Domain.Factories.UsuarioFactory.criar(
            "Teste", "teste@email.com", "SenhaCorreta123",
            Identidade.Domain.Enums.TipoUsuario.Vendedor);

        _repositorioMock.Setup(r => r.obterPorEmailAsync("teste@email.com", default))
            .ReturnsAsync(usuario);

        var dto = new LoginDto { email = "teste@email.com", senha = "SenhaErrada" };

        var resultado = await _servico.loginAsync(dto);

        resultado.foiSucesso.Should().BeFalse();
        resultado.erro.Should().Contain("inválid");
    }
}
