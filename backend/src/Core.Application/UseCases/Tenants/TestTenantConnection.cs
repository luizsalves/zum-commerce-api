using Core.Application.Contracts.Tenants;
using Core.Application.Interfaces;
using Core.Application.Services;

namespace Core.Application.UseCases.Tenants;

/// <summary>Confere se as credenciais da Shopify da loja funcionam.</summary>
public sealed class TestTenantConnection(TenantAccessService access, IShopifyShopGateway shop)
{
    public async Task<ShopInfoResponse> ExecuteAsync(string tenantId, CancellationToken ct)
    {
        access.RequirePlatformAdmin();
        var tenant = await access.RequireTenantAsync(tenantId, ct);
        var info = await shop.GetShopInfoAsync(tenant.Id, ct);
        return info with
        {
            Publications = info.Publications
                .Select(p => p with { Selected = tenant.PublicationIds.Contains(p.Id) })
                .ToList(),
        };
    }
}
