using Estoque.Domain.Excecoes;

namespace Estoque.Domain.Entidades;

public class ItemEstoque
{
    public Guid id { get; private set; }
    public Guid produtoId { get; private set; }
    public int quantidadeDisponivel { get; private set; }
    public DateTime ultimaAtualizacao { get; private set; }

    private ItemEstoque() { }

    public ItemEstoque(Guid id, Guid produtoId, int quantidadeInicial = 0)
    {
        if (produtoId == Guid.Empty)
            throw new DomainException("O identificador do produto é inválido.");

        this.id = id;
        this.produtoId = produtoId;
        this.quantidadeDisponivel = quantidadeInicial;
        this.ultimaAtualizacao = DateTime.UtcNow;
    }

    public void adicionarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade a adicionar deve ser maior que zero.");

        quantidadeDisponivel += quantidade;
        ultimaAtualizacao = DateTime.UtcNow;
    }

    public void baixarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade a baixar deve ser maior que zero.");

        if (quantidade > quantidadeDisponivel)
            throw new DomainException(
                $"Estoque insuficiente. Disponível: {quantidadeDisponivel}, solicitado: {quantidade}.");

        quantidadeDisponivel -= quantidade;
        ultimaAtualizacao = DateTime.UtcNow;
    }

    public bool temEstoqueSuficiente(int quantidade) => quantidadeDisponivel >= quantidade;
}
