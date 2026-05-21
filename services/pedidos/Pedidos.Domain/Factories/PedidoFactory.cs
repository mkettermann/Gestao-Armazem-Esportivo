using Pedidos.Domain.Entidades;

namespace Pedidos.Domain.Factories;

public static class PedidoFactory
{
    public static Pedido criar(string documentoCliente, string nomeVendedor)
        => Pedido.criar(Guid.NewGuid(), documentoCliente, nomeVendedor);
}
