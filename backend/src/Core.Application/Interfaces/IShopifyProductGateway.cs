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
}
