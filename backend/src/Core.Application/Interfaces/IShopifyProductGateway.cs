using Core.Domain.Catalog;
using Core.Domain.Common;

namespace Core.Application.Interfaces;

/// <summary>Produtos na Shopify (Admin API).</summary>
public interface IShopifyProductGateway
{
    Task<PagedResult<ProductSummary>> ListAsync(string tenantId, string? search, string? after, CancellationToken ct);
    Task<Product?> GetAsync(string tenantId, string productId, CancellationToken ct);

    /// <summary>Cria (Id nulo) ou atualiza o produto e devolve o id da Shopify.</summary>
    Task<string> SaveAsync(string tenantId, Product product, CancellationToken ct);

    Task SetStatusAsync(string tenantId, string productId, string status, CancellationToken ct);

    /// <summary>Publica (ou despublica) o produto nos canais de vendas da loja. Sem canais configurados, não faz nada.</summary>
    Task SyncPublicationsAsync(string tenantId, string productId, bool published, IReadOnlyList<string> publicationIds, CancellationToken ct);
}
