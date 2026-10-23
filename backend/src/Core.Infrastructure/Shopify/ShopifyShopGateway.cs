using Core.Application.Contracts.Tenants;
using Core.Application.Interfaces;

namespace Core.Infrastructure.Shopify;

public sealed class ShopifyShopGateway(ShopifyGraphQLClient client, ShopifyTokenProvider tokens) : IShopifyShopGateway
{
    public async Task<ShopInfoResponse> GetShopInfoAsync(string tenantId, CancellationToken ct)
    {
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.Shop, null, ct);
        var shop = data.GetProperty("shop");
        return new ShopInfoResponse(
            shop.Str("name") ?? "",
            shop.Str("myshopifyDomain") ?? "",
            shop.Str("currencyCode"),
            shop.Str("plan", "publicDisplayName"),
            data.GetProperty("publications").GetProperty("nodes").EnumerateArray()
                .Select(p => new ShopPublication(p.Str("id") ?? "", p.Str("catalog", "title") ?? "Canal sem nome", false))
                .ToList());
    }

    public void InvalidateToken(string tenantId) => tokens.Invalidate(tenantId);
}
