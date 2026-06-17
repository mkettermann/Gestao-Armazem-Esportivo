using Estoque.Application.DTOs;
using Estoque.Application.Mensageria;
using Estoque.Domain.Entidades;
using Estoque.Domain.Factories;
using Estoque.Domain.Interfaces;
using Shared.Contratos.Resultados;

namespace Estoque.Application.Servicos;

public class EstoqueServico
{
    private readonly IItemEstoqueRepositorio _itemRepositorio;
    private readonly IEntradaEstoqueRepositorio _entradaRepositorio;
    private readonly IEventoProcessadoRepositorio _eventosProcessados;

    public EstoqueServico(IItemEstoqueRepositorio itemRepositorio,
                          IEntradaEstoqueRepositorio entradaRepositorio,
                          IEventoProcessadoRepositorio eventosProcessados)
    {
        _itemRepositorio = itemRepositorio;
        _entradaRepositorio = entradaRepositorio;
        _eventosProcessados = eventosProcessados;
    }

    public async Task<Resultado<EntradaEstoqueRespostaDto>> adicionarEstoqueAsync(
        Guid produtoId, AdicionarEstoqueDto dto, CancellationToken ct = default)
    {
        Estoque.Domain.Entidades.EntradaEstoque entrada;
        try
        {
            entrada = EntradaEstoqueFactory.criar(produtoId, dto.quantidade, dto.numeroNotaFiscal);
        }
        catch (Estoque.Domain.Excecoes.DomainException ex)
        {
            return Resultado<EntradaEstoqueRespostaDto>.Falha(ex.Message);
        }

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

    /// <summary>
    /// Dá baixa nos itens de um pedido de forma idempotente e atômica (etapa da saga). Verifica a
    /// disponibilidade de TODOS os itens antes de aplicar qualquer baixa (sem baixa parcial); grava
    /// a baixa e o registro de idempotência na MESMA transação. Reentregas devolvem o resultado já
    /// processado sem reaplicar. Retorna se o pedido foi rejeitado e o motivo.
    /// </summary>
    public async Task<(bool rejeitado, string? motivo)> processarBaixaPedidoAsync(
        Guid idEvento, IReadOnlyList<ItemBaixa> itens, CancellationToken ct = default)
    {
        var jaProcessado = await _eventosProcessados.obterAsync(idEvento, ct);
        if (jaProcessado is not null)
            return (jaProcessado.rejeitado, jaProcessado.motivo);

        var aplicar = new List<(ItemEstoque item, int quantidade)>();
        foreach (var item in itens)
        {
            var estoqueItem = await _itemRepositorio.obterPorProdutoIdAsync(item.produtoId, ct);
            if (estoqueItem is null || !estoqueItem.temEstoqueSuficiente(item.quantidade))
            {
                var disponivel = estoqueItem?.quantidadeDisponivel ?? 0;
                var motivo = $"Estoque insuficiente para o produto {item.produtoId}. " +
                             $"Disponível: {disponivel}, solicitado: {item.quantidade}.";

                await _eventosProcessados.registrarAsync(idEvento, rejeitado: true, motivo, ct);
                await _itemRepositorio.salvarAlteracoesAsync(ct);
                return (true, motivo);
            }

            aplicar.Add((estoqueItem, item.quantidade));
        }

        foreach (var (item, quantidade) in aplicar)
            item.baixarQuantidade(quantidade);

        await _eventosProcessados.registrarAsync(idEvento, rejeitado: false, motivo: null, ct);
        await _itemRepositorio.salvarAlteracoesAsync(ct);

        return (false, null);
    }
}
