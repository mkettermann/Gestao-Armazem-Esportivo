namespace Estoque.Application.DTOs;

public sealed class AdicionarEstoqueDto
{
    public int quantidade { get; init; }
    public string numeroNotaFiscal { get; init; } = string.Empty;
}

public sealed class EstoqueRespostaDto
{
    public Guid produtoId { get; init; }
    public int quantidadeDisponivel { get; init; }
    public DateTime ultimaAtualizacao { get; init; }
}

public sealed class EntradaEstoqueRespostaDto
{
    public Guid produtoId { get; init; }
    public int quantidadeAdicionada { get; init; }
    public int quantidadeDisponivel { get; init; }
    public string numeroNotaFiscal { get; init; } = string.Empty;
    public DateTime dataEntrada { get; init; }
}
