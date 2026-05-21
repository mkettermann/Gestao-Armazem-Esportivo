using Estoque.Application.DTOs;
using Estoque.Domain.Factories;
using Estoque.Domain.Interfaces;
using Shared.Contratos.Resultados;

namespace Estoque.Application.Servicos;

public class EstoqueServico
{
    private readonly IItemEstoqueRepositorio _itemRepositorio;
    private readonly IEntradaEstoqueRepositorio _entradaRepositorio;

    public EstoqueServico(IItemEstoqueRepositorio itemRepositorio,
                          IEntradaEstoqueRepositorio entradaRepositorio)
    {
        _itemRepositorio = itemRepositorio;
        _entradaRepositorio = entradaRepositorio;
    }

    public async Task<Resultado<EntradaEstoqueRespostaDto>> adicionarEstoqueAsync(
        Guid produtoId, AdicionarEstoqueDto dto, CancellationToken ct = default)
    {
        var entrada = EntradaEstoqueFactory.criar(produtoId, dto.quantidade, dto.numeroNotaFiscal);

        var item = await _itemRepositorio.obterPorProdutoIdAsync(produtoId, ct);
        if (item is null)
        {
            item = ItemEstoqueFactory.criar(produtoId, dto.quantidade);
            await _itemRepositorio.adicionarAsync(item, ct);
        }
        else
        {
            item.adicionarQuantidade(dto.quantidade);
        }

        await _entradaRepositorio.adicionarAsync(entrada, ct);
        await _itemRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<EntradaEstoqueRespostaDto>.Sucesso(new EntradaEstoqueRespostaDto
        {
            produtoId = produtoId,
            quantidadeAdicionada = dto.quantidade,
            quantidadeDisponivel = item.quantidadeDisponivel,
            numeroNotaFiscal = dto.numeroNotaFiscal,
            dataEntrada = entrada.dataEntrada
        });
    }

    public async Task<Resultado<EstoqueRespostaDto>> obterEstoqueAsync(
        Guid produtoId, CancellationToken ct = default)
    {
        var item = await _itemRepositorio.obterPorProdutoIdAsync(produtoId, ct);
        if (item is null)
            return Resultado<EstoqueRespostaDto>.Sucesso(new EstoqueRespostaDto
            {
                produtoId = produtoId,
                quantidadeDisponivel = 0,
                ultimaAtualizacao = DateTime.UtcNow
            });

        return Resultado<EstoqueRespostaDto>.Sucesso(new EstoqueRespostaDto
        {
            produtoId = item.produtoId,
            quantidadeDisponivel = item.quantidadeDisponivel,
            ultimaAtualizacao = item.ultimaAtualizacao
        });
    }

    public async Task baixarEstoquePorPedidoAsync(
        Guid produtoId, int quantidade, CancellationToken ct = default)
    {
        var item = await _itemRepositorio.obterPorProdutoIdAsync(produtoId, ct)
            ?? throw new InvalidOperationException(
                $"Item de estoque não encontrado para o produto {produtoId}.");

        item.baixarQuantidade(quantidade);
        await _itemRepositorio.salvarAlteracoesAsync(ct);
    }
}
