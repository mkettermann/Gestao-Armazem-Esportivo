using Catalogo.Domain.Excecoes;
using Catalogo.Domain.ValueObjects;

namespace Catalogo.Domain.Entidades;

public class Produto
{
    public Guid id { get; private set; }
    public string nome { get; private set; } = string.Empty;
    public string descricao { get; private set; } = string.Empty;
    public Preco preco { get; private set; } = null!;
    public DateTime dataCadastro { get; private set; }
    public bool ativo { get; private set; }

    private Produto() { }

    internal Produto(Guid id, string nome, string descricao, Preco preco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        this.id = id;
        this.nome = nome.Trim();
        this.descricao = descricao?.Trim() ?? string.Empty;
        this.preco = preco;
        this.dataCadastro = DateTime.UtcNow;
        this.ativo = true;
    }

    public static Produto criar(Guid id, string nome, string descricao, Preco preco) =>
        new(id, nome, descricao, preco);

    public void atualizar(string nome, string descricao, Preco novoPreco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        this.nome = nome.Trim();
        this.descricao = descricao?.Trim() ?? string.Empty;
        this.preco = novoPreco;
    }

    public void desativar()
    {
        if (!ativo)
            throw new DomainException("O produto já está inativo.");
        ativo = false;
    }

    public void ativar()
    {
        if (ativo)
            throw new DomainException("O produto já está ativo.");
        ativo = true;
    }
}
