using Core.Application.Interfaces;
using Core.Domain.Catalog;
using Core.Domain.Common;

namespace Core.Web.Tests.Fakes;

/// <summary>Gateway de produtos em memória: registra as publicações pedidas.</summary>
public sealed class FakeProductGateway : IShopifyProductGateway
{
    public Dictionary<string, Product> Products { get; } = new();
    public List<(string ProductId, bool Published, IReadOnlyList<string> PublicationIds)> Syncs { get; } = new();

    public Task<PagedResult<ProductSummary>> ListAsync(string tenantId, string? search, string? after, CancellationToken ct) =>
        throw new NotImplementedException();

    public Task<Product?> GetAsync(string tenantId, string productId, CancellationToken ct) =>
        Task.FromResult(Products.GetValueOrDefault(productId));

    public Task<string> SaveAsync(string tenantId, Product product, CancellationToken ct)
    {
        var id = product.Id ?? "gid://shopify/Product/1";
        Products[id] = product;
        return Task.FromResult(id);
    }

    public Task SetStatusAsync(string tenantId, string productId, string status, CancellationToken ct) => Task.CompletedTask;

    public Task SyncPublicationsAsync(
        string tenantId, string productId, bool published, IReadOnlyList<string> publicationIds, CancellationToken ct)
    {
        Syncs.Add((productId, published, publicationIds));
        return Task.CompletedTask;
    }
}
