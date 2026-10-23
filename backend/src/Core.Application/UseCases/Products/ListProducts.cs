using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Catalog;
using Core.Domain.Common;

namespace Core.Application.UseCases.Products;

public sealed class ListProducts(TenantAccessService access, IShopifyProductGateway products)
{
    public async Task<PagedResult<ProductSummary>> ExecuteAsync(string? tenantId, string? search, string? after, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        return await products.ListAsync(tenant.Id, string.IsNullOrWhiteSpace(search) ? null : search.Trim(), after, ct);
    }
}
