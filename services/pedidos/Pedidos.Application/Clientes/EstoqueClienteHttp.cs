using Pedidos.Application.DTOs;
using Shared.Contratos.Respostas;
using Shared.Contratos.Resultados;
using System.Net.Http.Json;

namespace Pedidos.Application.Clientes;

public class EstoqueClienteHttp
{
    private readonly HttpClient _httpClient;

    public EstoqueClienteHttp(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Resultado<int>> obterQuantidadeDisponivel(
        Guid produtoId, CancellationToken ct = default)
    {
        try
        {
            var resposta = await _httpClient.GetAsync($"/estoque/{produtoId}", ct);
            if (!resposta.IsSuccessStatusCode)
                return Resultado<int>.Falha("Não foi possível consultar o estoque do produto.");

            var conteudo = await resposta.Content
                .ReadFromJsonAsync<RespostaApi<EstoqueRespostaInternoDto>>(ct);

            return conteudo?.sucesso == true
                ? Resultado<int>.Sucesso(conteudo.dados!.quantidadeDisponivel)
                : Resultado<int>.Falha(conteudo?.mensagem ?? "Erro ao consultar estoque.");
        }
        catch
        {
            return Resultado<int>.Falha(
                "Serviço de estoque temporariamente indisponível. Tente novamente em instantes.");
        }
    }
}

file sealed class EstoqueRespostaInternoDto
{
    public int quantidadeDisponivel { get; init; }
}
