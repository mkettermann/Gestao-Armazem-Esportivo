using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identidade.Domain.Entidades;
using Identidade.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identidade.Application.Servicos;

public class TokenServico
{
    private readonly IConfiguration _config;

    public TokenServico(IConfiguration config) => _config = config;

    public TokenRespostaDto gerarToken(Usuario usuario)
    {
        var chave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:Chave"]!));

        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiracaoHoras = int.Parse(_config["JWT:ExpiracaoHoras"] ?? "8");
        var expiracao = DateTime.UtcNow.AddHours(expiracaoHoras);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.nome),
            new Claim(ClaimTypes.Role, usuario.tipoUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JWT:Emissor"],
            audience: _config["JWT:Audiencia"],
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais);

        return new TokenRespostaDto
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiracao = expiracao,
            tipoUsuario = usuario.tipoUsuario.ToString()
        };
    }
}
