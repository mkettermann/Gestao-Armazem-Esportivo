using Catalogo.Domain.Entidades;
using Catalogo.Domain.ValueObjects;

namespace Catalogo.Domain.Factories;

public static class ProdutoFactory
{
    public static Produto criar(string nome, string descricao, decimal preco)
    {
        var precoVo = Preco.criar(preco);
        return Produto.criar(Guid.NewGuid(), nome, descricao, precoVo);
    }
}
