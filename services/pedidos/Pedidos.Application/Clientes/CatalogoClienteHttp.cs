using Pedidos.Application.DTOs;
using Shared.Contratos.Respostas;
using Shared.Contratos.Resultados;
using System.Net.Http.Json;

namespace Pedidos.Application.Clientes;

public class CatalogoClienteHttp
{
    private readonly HttpClient _httpClient;

    public CatalogoClienteHttp(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Resultado<ProdutoExternoDto>> obterProdutoAsync(
        Guid produtoId, CancellationToken ct = default)
    {
        try
        {
            var resposta = await _httpClient.GetAsync($"/produtos/{produtoId}", ct);
            if (!resposta.IsSuccessStatusCode)
                return Resultado<ProdutoExternoDto>.Falha(
                    $"Produto {produtoId} não encontrado no catálogo.");

            var conteudo = await resposta.Content
                .ReadFromJsonAsync<RespostaApi<ProdutoExternoDto>>(ct);

            return conteudo?.sucesso == true && conteudo.dados is not null
                ? Resultado<ProdutoExternoDto>.Sucesso(conteudo.dados)
                : Resultado<ProdutoExternoDto>.Falha(
                    conteudo?.mensagem ?? "Erro ao consultar o catálogo de produtos.");
        }
        catch
        {
            return Resultado<ProdutoExternoDto>.Falha(
                "Serviço de catálogo temporariamente indisponível. Tente novamente em instantes.");
        }
    }
}
