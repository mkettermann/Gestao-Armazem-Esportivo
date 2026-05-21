using Pedidos.Domain.Excecoes;

namespace Pedidos.Domain.Entidades;

public class ItemPedido
{
    public Guid id { get; private set; }
    public Guid pedidoId { get; private set; }
    public Guid produtoId { get; private set; }
    public string nomeProduto { get; private set; } = string.Empty;
    public decimal precoUnitario { get; private set; }
    public int quantidade { get; private set; }

    /// <summary>Subtotal calculado pelo domínio: preço unitário × quantidade.</summary>
    public decimal subtotal => precoUnitario * quantidade;

    private ItemPedido() { }

    internal ItemPedido(Guid id, Guid pedidoId, Guid produtoId,
                        string nomeProduto, decimal precoUnitario, int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade do item deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("O preço unitário do item deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(nomeProduto))
            throw new DomainException("O nome do produto no item é obrigatório.");

        this.id = id;
        this.pedidoId = pedidoId;
        this.produtoId = produtoId;
        this.nomeProduto = nomeProduto.Trim();
        this.precoUnitario = precoUnitario;
        this.quantidade = quantidade;
    }
}
