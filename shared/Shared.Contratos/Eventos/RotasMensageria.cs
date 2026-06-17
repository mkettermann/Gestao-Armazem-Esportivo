namespace Shared.Contratos.Eventos;

/// <summary>
/// Nomes centralizados de exchanges, filas e routing keys da saga Pedido → Estoque → Pedido.
/// Evita "strings mágicas" espalhadas entre publicadores e consumidores.
/// </summary>
public static class RotasMensageria
{
    // Pedidos -> Estoque
    public const string ExchangePedidos = "pedidos.events";
    public const string RoutingPedidoRegistrado = "pedidos.pedido.registrado";
    public const string FilaEstoquePedidoRegistrado = "estoque.pedido-registrado";

    // Estoque -> Pedidos
    public const string ExchangeEstoque = "estoque.events";
    public const string RoutingEstoqueBaixado = "estoque.baixado";
    public const string RoutingEstoqueRejeitado = "estoque.rejeitado";
    public const string FilaPedidosRespostaEstoque = "pedidos.resposta-estoque";

    // Dead-letter compartilhado
    public const string ExchangeDlx = "gestao.dlx";
}
