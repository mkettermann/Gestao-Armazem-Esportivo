using Asp.Versioning;
using Pedidos.Application.DTOs;
using Pedidos.Application.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contratos.Respostas;

namespace Pedidos.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("pedidos")]
[Authorize]
public class PedidoController : ControllerBase
{
    private readonly PedidoServico _pedidoServico;

    public PedidoController(PedidoServico pedidoServico)
        => _pedidoServico = pedidoServico;

    [HttpPost]
    [Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> emitir([FromBody] CriarPedidoDto dto, CancellationToken ct)
    {
        var resultado = await _pedidoServico.emitirAsync(dto, ct);
        if (!resultado.foiSucesso)
        {
            var status = resultado.erro!.Contains("insuficiente") ? 422 : 400;
            return StatusCode(status, RespostaApi<PedidoRespostaDto>.Erro(resultado.erro));
        }

        return CreatedAtAction(nameof(obterPorId), new { id = resultado.valor!.id },
            RespostaApi<PedidoRespostaDto>.Ok(resultado.valor, "Pedido emitido com sucesso."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> obterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _pedidoServico.obterPorIdAsync(id, ct);
        if (!resultado.foiSucesso)
            return NotFound(RespostaApi<PedidoRespostaDto>.Erro(resultado.erro!));

        return Ok(RespostaApi<PedidoRespostaDto>.Ok(resultado.valor!));
    }
}
