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
            shop.Str("plan", "publicDisplayName"));
    }

    public void InvalidateToken(string tenantId) => tokens.Invalidate(tenantId);
}
