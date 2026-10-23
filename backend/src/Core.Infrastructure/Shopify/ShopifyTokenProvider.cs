using System.Text.Json;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Shopify;

/// <summary>
/// Access token da Admin API por loja.
/// client_credentials: troca client_id/secret por um token de ~24h, guardado em cache.
/// access_token: usa o token salvo (app OAuth).
/// </summary>
public sealed class ShopifyTokenProvider(
    ITenantRepository tenants,
    ISecretProtector protector,
    IHttpClientFactory httpFactory,
    IMemoryCache cache,
    ILogger<ShopifyTokenProvider> logger)
{
    public async Task<string> GetTokenAsync(string tenantId, CancellationToken ct)
    {
        if (cache.TryGetValue<string>(Key(tenantId), out var cached) && cached is not null) return cached;

        var tenant = await tenants.GetAsync(tenantId, ct) ?? throw AppException.NotFound("Loja não encontrada.");
        var credentials = await tenants.GetCredentialsAsync(tenantId, ct)
            ?? throw AppException.Unprocessable("Esta loja ainda não tem credenciais da Shopify configuradas.");

        if (credentials.Mode == CredentialModes.AccessToken)
        {
            if (string.IsNullOrEmpty(credentials.AccessTokenEncrypted))
                throw AppException.Unprocessable("Token de acesso da loja não configurado.");
            var token = protector.Unprotect(credentials.AccessTokenEncrypted);
            cache.Set(Key(tenantId), token, TimeSpan.FromHours(1));
            return token;
        }

        if (string.IsNullOrEmpty(credentials.ClientId) || string.IsNullOrEmpty(credentials.ClientSecretEncrypted))
            throw AppException.Unprocessable("Client ID/Secret da loja não configurados.");

        var http = httpFactory.CreateClient(ShopifyHttp.ClientName);
        using var response = await http.PostAsync(
            $"https://{tenant.ShopDomain}/admin/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = credentials.ClientId,
                ["client_secret"] = protector.Unprotect(credentials.ClientSecretEncrypted),
            }),
            ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Falha ao obter token da loja {Tenant}: {Status} {Body}", tenantId, (int)response.StatusCode, body);
            throw AppException.External(
                "Não foi possível autenticar na Shopify. Confira o Client ID/Secret e se o app está instalado na loja.");
        }

        using var json = JsonDocument.Parse(body);
        var accessToken = json.RootElement.GetProperty("access_token").GetString()
            ?? throw AppException.External("Resposta de token inválida da Shopify.");
        var expiresIn = json.RootElement.TryGetProperty("expires_in", out var exp) && exp.TryGetInt32(out var secs)
            ? secs
            : 86_399;

        // Renova 10 minutos antes de expirar.
        cache.Set(Key(tenantId), accessToken, TimeSpan.FromSeconds(Math.Max(60, expiresIn - 600)));
        return accessToken;
    }

    public void Invalidate(string tenantId) => cache.Remove(Key(tenantId));

    private static string Key(string tenantId) => $"shopify-token:{tenantId}";
}
