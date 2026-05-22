namespace Pedidos.Application.DTOs;

public sealed class ItemPedidoDto
{
    public Guid produtoId { get; init; }
    public int quantidade { get; init; }
}

public sealed class CriarPedidoDto
{
    public string documentoCliente { get; init; } = string.Empty;
    public string nomeVendedor { get; init; } = string.Empty;
    public List<ItemPedidoDto> itens { get; init; } = new();
}

public sealed class ItemPedidoRespostaDto
{
    public Guid produtoId { get; init; }
    public string nomeProduto { get; init; } = string.Empty;
    public decimal precoUnitario { get; init; }
    public int quantidade { get; init; }
    public decimal subtotal { get; init; }
}

public sealed class PedidoRespostaDto
{
    public Guid id { get; init; }
    public string documentoCliente { get; init; } = string.Empty;
    public string nomeVendedor { get; init; } = string.Empty;
    public string status { get; init; } = string.Empty;
    public DateTime dataCriacao { get; init; }
    public DateTime? dataConfirmacao { get; init; }
    public IEnumerable<ItemPedidoRespostaDto> itens { get; init; } = [];
    public decimal valorTotal { get; init; }
}

public sealed class ProdutoExternoDto
{
    public Guid id { get; init; }
    public string nome { get; init; } = string.Empty;
    public decimal preco { get; init; }
}
