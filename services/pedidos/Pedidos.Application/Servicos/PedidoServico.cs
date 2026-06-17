using Pedidos.Application.Clientes;
using Pedidos.Application.DTOs;
using Pedidos.Application.Mensageria;
using Pedidos.Domain.Entidades;
using Pedidos.Domain.Factories;
using Pedidos.Domain.Interfaces;
using Shared.Contratos.Eventos;
using Shared.Contratos.Resultados;

namespace Pedidos.Application.Servicos;

public class PedidoServico
{
    private readonly IPedidoRepositorio _pedidoRepositorio;
    private readonly EstoqueClienteHttp _estoqueCliente;
    private readonly CatalogoClienteHttp _catalogoCliente;
    private readonly IOutboxRepositorio _outbox;

    public PedidoServico(IPedidoRepositorio pedidoRepositorio,
                         EstoqueClienteHttp estoqueCliente,
                         CatalogoClienteHttp catalogoCliente,
                         IOutboxRepositorio outbox)
    {
        _pedidoRepositorio = pedidoRepositorio;
        _estoqueCliente = estoqueCliente;
        _catalogoCliente = catalogoCliente;
        _outbox = outbox;
    }

    public async Task<Resultado<PedidoRespostaDto>> emitirAsync(
        CriarPedidoDto dto, CancellationToken ct = default)
    {
        if (dto.itens is null || !dto.itens.Any())
            return Resultado<PedidoRespostaDto>.Falha("O pedido deve conter ao menos um item.");

        // Pré-validação síncrona: dá feedback imediato (H5) no caso comum de estoque insuficiente.
        // A baixa autoritativa e a confirmação do pedido ocorrem na saga assíncrona com o Estoque.
        var produtosConsultados = new Dictionary<Guid, ProdutoExternoDto>();

        foreach (var item in dto.itens)
        {
            var catalogoResultado = await _catalogoCliente.obterProdutoAsync(item.produtoId, ct);
            if (!catalogoResultado.foiSucesso)
                return Resultado<PedidoRespostaDto>.Falha(catalogoResultado.erro!);

            var estoqueResultado = await _estoqueCliente.obterQuantidadeDisponivel(item.produtoId, ct);
            if (!estoqueResultado.foiSucesso)
                return Resultado<PedidoRespostaDto>.Falha(estoqueResultado.erro!);

            if (estoqueResultado.valor < item.quantidade)
                return Resultado<PedidoRespostaDto>.Falha(
                    $"Estoque insuficiente para '{catalogoResultado.valor!.nome}'. " +
                    $"Disponível: {estoqueResultado.valor}, solicitado: {item.quantidade}.");

            produtosConsultados[item.produtoId] = catalogoResultado.valor!;
        }

        var pedido = PedidoFactory.criar(dto.documentoCliente, dto.nomeVendedor);

        foreach (var item in dto.itens)
        {
            var produto = produtosConsultados[item.produtoId];
            pedido.adicionarItem(item.produtoId, produto.nome, produto.preco, item.quantidade);
        }

        // O pedido nasce Pendente; só vira Confirmado quando o Estoque confirmar a baixa (saga).
        var evento = new PedidoRegistradoEvento
        {
            pedidoId = pedido.id,
            dataRegistro = pedido.dataCriacao,
            itens = dto.itens.Select(i => new ItemPedidoEvento
            {
                produtoId = i.produtoId,
                quantidade = i.quantidade
            }).ToList()
        };

        // Pedido + mensagem de outbox gravados na MESMA transação (sem dual-write).
        await _pedidoRepositorio.adicionarAsync(pedido, ct);
        await _outbox.adicionarAsync(evento, RotasMensageria.ExchangePedidos,
                                     RotasMensageria.RoutingPedidoRegistrado, ct);
        await _pedidoRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<PedidoRespostaDto>.Sucesso(mapear(pedido));
    }

    public async Task<Resultado<PedidoRespostaDto>> obterPorIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var pedido = await _pedidoRepositorio.obterPorIdAsync(id, ct);
        if (pedido is null)
            return Resultado<PedidoRespostaDto>.Falha("Pedido não encontrado.");

        return Resultado<PedidoRespostaDto>.Sucesso(mapear(pedido));
    }

    private static PedidoRespostaDto mapear(Pedido p) => new()
    {
        id = p.id,
        documentoCliente = p.documentoCliente,
        nomeVendedor = p.nomeVendedor,
        status = p.status.ToString(),
        dataCriacao = p.dataCriacao,
        dataConfirmacao = p.dataConfirmacao,
        motivoRejeicao = p.motivoRejeicao,
        itens = p.itens.Select(i => new ItemPedidoRespostaDto
        {
            produtoId = i.produtoId,
            nomeProduto = i.nomeProduto,
            precoUnitario = i.precoUnitario,
            quantidade = i.quantidade,
            subtotal = i.subtotal
        }),
        valorTotal = p.calcularTotal()
    };
}
