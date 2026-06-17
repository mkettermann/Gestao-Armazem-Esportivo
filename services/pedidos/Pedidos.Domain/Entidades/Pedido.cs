using Pedidos.Domain.Enums;
using Pedidos.Domain.Excecoes;
using Pedidos.Domain.ValueObjects;

namespace Pedidos.Domain.Entidades;

public class Pedido
{
    public Guid id { get; private set; }
    public DocumentoCliente documentoCliente { get; private set; } = null!;
    public string nomeVendedor { get; private set; } = string.Empty;
    public StatusPedido status { get; private set; }
    public DateTime dataCriacao { get; private set; }
    public DateTime? dataConfirmacao { get; private set; }
    public DateTime? dataCancelamento { get; private set; }
    public string? motivoRejeicao { get; private set; }

    private readonly List<ItemPedido> _itens = new();
    public IReadOnlyCollection<ItemPedido> itens => _itens.AsReadOnly();

    private Pedido() { }

    internal Pedido(Guid id, string documentoCliente, string nomeVendedor)
    {
        if (string.IsNullOrWhiteSpace(nomeVendedor))
            throw new DomainException("O nome do vendedor é obrigatório.");

        this.id = id;
        this.documentoCliente = DocumentoCliente.criar(documentoCliente);
        this.nomeVendedor = nomeVendedor.Trim();
        this.status = StatusPedido.Pendente;
        this.dataCriacao = DateTime.UtcNow;
    }

    public static Pedido criar(Guid id, string documentoCliente, string nomeVendedor) =>
        new(id, documentoCliente, nomeVendedor);

    public void adicionarItem(Guid produtoId, string nomeProduto,
                              decimal precoUnitario, int quantidade)
    {
        if (_itens.Any(i => i.produtoId == produtoId))
            throw new DomainException("O produto já foi adicionado ao pedido.");

        _itens.Add(new ItemPedido(Guid.NewGuid(), id, produtoId,
                                  nomeProduto, precoUnitario, quantidade));
    }

    public void confirmar()
    {
        if (!_itens.Any())
            throw new DomainException("O pedido não pode ser confirmado sem itens.");
        if (status != StatusPedido.Pendente)
            throw new DomainException("Apenas pedidos pendentes podem ser confirmados.");

        status = StatusPedido.Confirmado;
        dataConfirmacao = DateTime.UtcNow;
    }

    public void cancelar()
    {
        if (status == StatusPedido.Cancelado)
            throw new DomainException("O pedido já está cancelado.");
        if (status == StatusPedido.Confirmado)
            throw new DomainException("Pedidos já confirmados não podem ser cancelados.");

        status = StatusPedido.Cancelado;
        dataCancelamento = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejeita um pedido pendente quando o estoque não pôde ser baixado (etapa de compensação da
    /// saga). Mantém o histórico do motivo para consulta pelo vendedor.
    /// </summary>
    public void rejeitar(string motivo)
    {
        if (status != StatusPedido.Pendente)
            throw new DomainException("Apenas pedidos pendentes podem ser rejeitados.");

        status = StatusPedido.Rejeitado;
        dataCancelamento = DateTime.UtcNow;
        motivoRejeicao = string.IsNullOrWhiteSpace(motivo) ? "Estoque insuficiente." : motivo.Trim();
    }

    public decimal calcularTotal() => _itens.Sum(i => i.subtotal);
}
