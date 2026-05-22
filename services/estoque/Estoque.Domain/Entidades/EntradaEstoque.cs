using Estoque.Domain.Excecoes;

namespace Estoque.Domain.Entidades;

public class EntradaEstoque
{
    public Guid id { get; private set; }
    public Guid produtoId { get; private set; }
    public int quantidade { get; private set; }
    public string numeroNotaFiscal { get; private set; } = string.Empty;
    public DateTime dataEntrada { get; private set; }

    private EntradaEstoque() { }

    public EntradaEstoque(Guid id, Guid produtoId, int quantidade, string numeroNotaFiscal)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade da entrada deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(numeroNotaFiscal))
            throw new DomainException("O número da nota fiscal é obrigatório.");

        this.id = id;
        this.produtoId = produtoId;
        this.quantidade = quantidade;
        this.numeroNotaFiscal = numeroNotaFiscal.Trim();
        this.dataEntrada = DateTime.UtcNow;
    }
}
