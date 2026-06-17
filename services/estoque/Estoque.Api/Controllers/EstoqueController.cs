using Asp.Versioning;
using Estoque.Application.DTOs;
using Estoque.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contratos.Respostas;

namespace Estoque.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("estoque")]
[Authorize]
public class EstoqueController : ControllerBase
{
    private readonly EstoqueServico _estoqueServico;

    public EstoqueController(EstoqueServico estoqueServico)
        => _estoqueServico = estoqueServico;

    [HttpPost("{produtoId:guid}/entradas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> adicionarEstoque(Guid produtoId,
                                                      [FromBody] AdicionarEstoqueDto dto,
                                                      CancellationToken ct)
    {
        var resultado = await _estoqueServico.adicionarEstoqueAsync(produtoId, dto, ct);
        if (!resultado.foiSucesso)
            return BadRequest(RespostaApi<EntradaEstoqueRespostaDto>.Erro(resultado.erro!));

        return Ok(RespostaApi<EntradaEstoqueRespostaDto>.Ok(resultado.valor!, "Estoque adicionado com sucesso."));
    }

    [HttpGet("{produtoId:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> obterEstoque(Guid produtoId, CancellationToken ct)
    {
        var resultado = await _estoqueServico.obterEstoqueAsync(produtoId, ct);
        return Ok(RespostaApi<EstoqueRespostaDto>.Ok(resultado.valor!));
    }
}
