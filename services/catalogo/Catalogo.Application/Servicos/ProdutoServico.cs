using Catalogo.Application.DTOs;
using Catalogo.Domain.Factories;
using Catalogo.Domain.Interfaces;
using Catalogo.Domain.ValueObjects;
using Shared.Contratos.Resultados;

namespace Catalogo.Application.Servicos;

public class ProdutoServico
{
    private readonly IProdutoRepositorio _produtoRepositorio;

    public ProdutoServico(IProdutoRepositorio produtoRepositorio)
        => _produtoRepositorio = produtoRepositorio;

    public async Task<Resultado<ProdutoRespostaDto>> criarAsync(
        CriarProdutoDto dto, CancellationToken ct = default)
    {
        var produto = ProdutoFactory.criar(dto.nome, dto.descricao, dto.preco);
        await _produtoRepositorio.adicionarAsync(produto, ct);
        await _produtoRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<ProdutoRespostaDto>.Sucesso(mapear(produto));
    }

    public async Task<Resultado<ListaProdutosRespostaDto>> listarAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var (itens, total) = await _produtoRepositorio.listarAsync(pagina, tamanhoPagina, ct);
        return Resultado<ListaProdutosRespostaDto>.Sucesso(new ListaProdutosRespostaDto
        {
            itens = itens.Select(mapear),
            totalItens = total,
            pagina = pagina,
            tamanhoPagina = tamanhoPagina
        });
    }

    public async Task<Resultado<ProdutoRespostaDto>> obterPorIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var produto = await _produtoRepositorio.obterPorIdAsync(id, ct);
        if (produto is null)
            return Resultado<ProdutoRespostaDto>.Falha("Produto não encontrado.");

        return Resultado<ProdutoRespostaDto>.Sucesso(mapear(produto));
    }

    public async Task<Resultado<ProdutoRespostaDto>> atualizarAsync(
        Guid id, AtualizarProdutoDto dto, CancellationToken ct = default)
    {
        var produto = await _produtoRepositorio.obterPorIdAsync(id, ct);
        if (produto is null)
            return Resultado<ProdutoRespostaDto>.Falha("Produto não encontrado.");

        produto.atualizar(dto.nome, dto.descricao, Preco.criar(dto.preco));
        await _produtoRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<ProdutoRespostaDto>.Sucesso(mapear(produto));
    }

    public async Task<Resultado<bool>> removerAsync(Guid id, CancellationToken ct = default)
    {
        var produto = await _produtoRepositorio.obterPorIdAsync(id, ct);
        if (produto is null)
            return Resultado<bool>.Falha("Produto não encontrado.");

        produto.desativar();
        await _produtoRepositorio.salvarAlteracoesAsync(ct);

        return Resultado<bool>.Sucesso(true);
    }

    private static ProdutoRespostaDto mapear(Domain.Entidades.Produto p) => new()
    {
        id = p.id,
        nome = p.nome,
        descricao = p.descricao,
        preco = p.preco,
        dataCadastro = p.dataCadastro
    };
}
