using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Shopify;

/// <summary>Cliente GraphQL da Admin API. Renova o token se expirar e espera em caso de limite de taxa.</summary>
public sealed class ShopifyGraphQLClient(
    ITenantRepository tenants,
    ShopifyTokenProvider tokens,
    IHttpClientFactory httpFactory,
    ILogger<ShopifyGraphQLClient> logger)
{
    private const int MaxAttempts = 3;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Executa a operação e devolve o nó "data".</summary>
    public async Task<JsonElement> ExecuteAsync(string tenantId, string query, object? variables, CancellationToken ct)
    {
        var tenant = await tenants.GetAsync(tenantId, ct) ?? throw AppException.NotFound("Loja não encontrada.");
        var url = $"https://{tenant.ShopDomain}/admin/api/{tenant.ApiVersion}/graphql.json";
        var payload = JsonSerializer.Serialize(new { query, variables }, JsonOptions);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var token = await tokens.GetTokenAsync(tenantId, ct);
            var http = httpFactory.CreateClient(ShopifyHttp.ClientName);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("X-Shopify-Access-Token", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 1)
            {
                tokens.Invalidate(tenantId);
                continue;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests || body.Contains("\"THROTTLED\"", StringComparison.Ordinal))
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Shopify {Status} para {Tenant}: {Body}", (int)response.StatusCode, tenantId, body);
                throw AppException.External($"A Shopify respondeu com erro ({(int)response.StatusCode}).");
            }

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("errors", out var errors)
                && errors.ValueKind == JsonValueKind.Array
                && errors.GetArrayLength() > 0)
            {
                logger.LogWarning("Erros GraphQL para {Tenant}: {Errors}", tenantId, errors.ToString());
                var first = errors[0].TryGetProperty("message", out var m) ? m.GetString() : "erro desconhecido";
                throw AppException.External($"Erro na consulta à Shopify: {first}");
            }

            return doc.RootElement.GetProperty("data").Clone();
        }

        throw AppException.External("A Shopify está limitando as requisições. Tente de novo em alguns segundos.");
    }

    /// <summary>Converte userErrors de uma mutation em erro de validação legível.</summary>
    public static void ThrowOnUserErrors(JsonElement payload)
    {
        if (!payload.TryGetProperty("userErrors", out var errors) || errors.ValueKind != JsonValueKind.Array) return;
        if (errors.GetArrayLength() == 0) return;

        var messages = errors.EnumerateArray()
            .Select(e => e.TryGetProperty("message", out var m) ? m.GetString() : null)
            .Where(m => !string.IsNullOrWhiteSpace(m));
        throw AppException.Unprocessable("Shopify: " + string.Join(" | ", messages));
    }
}
