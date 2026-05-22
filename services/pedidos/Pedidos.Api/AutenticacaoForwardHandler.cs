using Microsoft.AspNetCore.Http;

namespace Pedidos.Api;

/// <summary>
/// DelegatingHandler que propaga o token JWT da requisição HTTP atual
/// para chamadas internas de serviço a serviço.
/// </summary>
public class AutenticacaoForwardHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AutenticacaoForwardHandler(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext?
            .Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        return base.SendAsync(request, cancellationToken);
    }
}
