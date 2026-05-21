using Pedidos.Application.Clientes;
using Pedidos.Application.DTOs;
using Pedidos.Domain.Entidades;
using Pedidos.Domain.Factories;
using Pedidos.Domain.Interfaces;
using Shared.Contratos.Eventos;
using Shared.Contratos.Resultados;

namespace Pedidos.Application.Servicos;

public interface IEventoPublicador
{
    Task publicarAsync<T>(T evento, string exchange, string routingKey,
                          CancellationToken ct = default);
}

public class PedidoServico
{
    private readonly IPedidoRepositorio _pedidoRepositorio;
    private readonly EstoqueClienteHttp _estoqueCliente;
    private readonly CatalogoClienteHttp _catalogoCliente;
    private readonly IEventoPublicador _publicador;

    public PedidoServico(IPedidoRepositorio pedidoRepositorio,
                         EstoqueClienteHttp estoqueCliente,
                         CatalogoClienteHttp catalogoCliente,
                         IEventoPublicador publicador)
    {
        _pedidoRepositorio = pedidoRepositorio;
        _estoqueCliente = estoqueCliente;
        _catalogoCliente = catalogoCliente;
        _publicador = publicador;
    }

    public async Task<Resultado<PedidoRespostaDto>> emitirAsync(
        CriarPedidoDto dto, CancellationToken ct = default)
    {
        if (dto.itens is null || !dto.itens.Any())
            return Resultado<PedidoRespostaDto>.Falha("O pedido deve conter ao menos um item.");

        // Consulta catálogo e valida estoque de todos os itens antes de criar o pedido
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

        pedido.confirmar();

        await _pedidoRepositorio.adicionarAsync(pedido, ct);
        await _pedidoRepositorio.salvarAlteracoesAsync(ct);

        var evento = new PedidoConfirmadoEvento
        {
            pedidoId = pedido.id,
            dataConfirmacao = pedido.dataConfirmacao!.Value,
            itens = dto.itens.Select(i => new ItemPedidoEvento
            {
                produtoId = i.produtoId,
                quantidade = i.quantidade
            }).ToList()
        };

        await _publicador.publicarAsync(evento, "pedidos.events",
                                        "pedidos.pedido.confirmado", ct);

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
