using Identidade.Application.DTOs;
using Identidade.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contratos.Respostas;

namespace Identidade.Api.Controllers;

[ApiController]
[Route("auth")]
public class AutenticacaoController : ControllerBase
{
    private readonly AutenticacaoServico _autenticacaoServico;

    public AutenticacaoController(AutenticacaoServico autenticacaoServico)
        => _autenticacaoServico = autenticacaoServico;

    [HttpPost("usuarios")]
    [AllowAnonymous]
    public async Task<IActionResult> cadastrar([FromBody] CadastrarUsuarioDto dto,
                                               CancellationToken ct)
    {
        var resultado = await _autenticacaoServico.cadastrarAsync(dto, ct);
        if (!resultado.foiSucesso)
            return Conflict(RespostaApi<UsuarioRespostaDto>.Erro(resultado.erro!));

        return CreatedAtAction(nameof(cadastrar), null,
            RespostaApi<UsuarioRespostaDto>.Ok(resultado.valor!, "Usuário cadastrado com sucesso."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var resultado = await _autenticacaoServico.loginAsync(dto, ct);
        if (!resultado.foiSucesso)
            return Unauthorized(RespostaApi<TokenRespostaDto>.Erro(resultado.erro!));

        return Ok(RespostaApi<TokenRespostaDto>.Ok(resultado.valor!, "Login realizado com sucesso."));
    }
}
