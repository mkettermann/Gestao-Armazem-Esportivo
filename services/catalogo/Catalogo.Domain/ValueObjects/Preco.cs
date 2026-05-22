using Catalogo.Domain.Excecoes;

namespace Catalogo.Domain.ValueObjects;

/// <summary>
/// Value object que encapsula e valida um preço monetário.
/// Garante que nenhum valor inválido (zero ou negativo) seja aceito pelo domínio.
/// </summary>
public sealed record Preco
{
    public decimal valor { get; }

    private Preco(decimal valor) => this.valor = valor;

    public static Preco criar(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("O preço deve ser maior que zero.");

        return new Preco(decimal.Round(valor, 2));
    }

    public static implicit operator decimal(Preco preco) => preco.valor;
    public override string ToString() => valor.ToString("C");
}
