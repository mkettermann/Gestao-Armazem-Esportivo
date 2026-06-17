using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Integracao.Tests;

/// <summary>
/// Testa o fluxo de autenticação (H1 + H2) ponta a ponta pelo pipeline HTTP real: validação
/// (FluentValidation), envelope <c>RespostaApi</c>, hashing e emissão de token.
/// </summary>
public class AutenticacaoFluxoTests : IClassFixture<FabricaIdentidadeApi>
{
    private readonly HttpClient _client;

    public AutenticacaoFluxoTests(FabricaIdentidadeApi fabrica) => _client = fabrica.CreateClient();

    [Fact]
    public async Task CadastrarELogar_DeveRetornarToken()
    {
        var email = $"admin_{Guid.NewGuid():N}@armazem.com";

        var cadastro = await _client.PostAsJsonAsync("/auth/usuarios", new
        {
            nome = "Admin",
            email,
            senha = "Senha@123",
            tipoUsuario = "Administrador"
        });
        cadastro.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await _client.PostAsJsonAsync("/auth/login", new { email, senha = "Senha@123" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("dados").GetProperty("token").GetString()
            .Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Cadastrar_DeveRetornar400NoEnvelope_QuandoSenhaCurta()
    {
        var resposta = await _client.PostAsJsonAsync("/auth/usuarios", new
        {
            nome = "Teste",
            email = $"curta_{Guid.NewGuid():N}@armazem.com",
            senha = "123",
            tipoUsuario = "Vendedor"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("mensagem").GetString().Should().Contain("6 caracteres");
    }

    [Fact]
    public async Task Login_DeveRetornar401_QuandoCredenciaisInvalidas()
    {
        var resposta = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = $"naoexiste_{Guid.NewGuid():N}@armazem.com",
            senha = "qualquer123"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
