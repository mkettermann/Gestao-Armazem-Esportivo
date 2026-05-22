using Catalogo.Application.DTOs;
using Catalogo.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contratos.Respostas;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("produtos")]
[Authorize]
public class ProdutoController : ControllerBase
{
    private readonly ProdutoServico _produtoServico;

    public ProdutoController(ProdutoServico produtoServico)
        => _produtoServico = produtoServico;

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> criar([FromBody] CriarProdutoDto dto, CancellationToken ct)
    {
        var resultado = await _produtoServico.criarAsync(dto, ct);
        if (!resultado.foiSucesso)
            return BadRequest(RespostaApi<ProdutoRespostaDto>.Erro(resultado.erro!));

        return CreatedAtAction(nameof(obterPorId), new { id = resultado.valor!.id },
            RespostaApi<ProdutoRespostaDto>.Ok(resultado.valor, "Produto cadastrado com sucesso."));
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> listar([FromQuery] int pagina = 1,
                                             [FromQuery] int tamanhoPagina = 20,
                                             CancellationToken ct = default)
    {
        var resultado = await _produtoServico.listarAsync(pagina, tamanhoPagina, ct);
        return Ok(RespostaApi<ListaProdutosRespostaDto>.Ok(resultado.valor!));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> obterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _produtoServico.obterPorIdAsync(id, ct);
        if (!resultado.foiSucesso)
            return NotFound(RespostaApi<ProdutoRespostaDto>.Erro(resultado.erro!));

        return Ok(RespostaApi<ProdutoRespostaDto>.Ok(resultado.valor!));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> atualizar(Guid id, [FromBody] AtualizarProdutoDto dto,
                                               CancellationToken ct)
    {
        var resultado = await _produtoServico.atualizarAsync(id, dto, ct);
        if (!resultado.foiSucesso)
            return NotFound(RespostaApi<ProdutoRespostaDto>.Erro(resultado.erro!));

        return Ok(RespostaApi<ProdutoRespostaDto>.Ok(resultado.valor!, "Produto atualizado com sucesso."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> remover(Guid id, CancellationToken ct)
    {
        var resultado = await _produtoServico.removerAsync(id, ct);
        if (!resultado.foiSucesso)
            return NotFound(RespostaApi.Erro(resultado.erro!));

        return Ok(RespostaApi.Ok("Produto removido com sucesso."));
    }
}
