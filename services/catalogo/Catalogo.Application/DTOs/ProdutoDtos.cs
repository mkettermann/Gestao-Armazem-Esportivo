namespace Catalogo.Application.DTOs;

public sealed class CriarProdutoDto
{
    public string nome { get; init; } = string.Empty;
    public string descricao { get; init; } = string.Empty;
    public decimal preco { get; init; }
}

public sealed class AtualizarProdutoDto
{
    public string nome { get; init; } = string.Empty;
    public string descricao { get; init; } = string.Empty;
    public decimal preco { get; init; }
}

public sealed class ProdutoRespostaDto
{
    public Guid id { get; init; }
    public string nome { get; init; } = string.Empty;
    public string descricao { get; init; } = string.Empty;
    public decimal preco { get; init; }
    public DateTime dataCadastro { get; init; }
}

public sealed class ListaProdutosRespostaDto
{
    public IEnumerable<ProdutoRespostaDto> itens { get; init; } = [];
    public int totalItens { get; init; }
    public int pagina { get; init; }
    public int tamanhoPagina { get; init; }
}
