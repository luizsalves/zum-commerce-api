using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Constants;

namespace Core.Application.UseCases.Products;

public sealed class SetProductStatus(TenantAccessService access, IShopifyProductGateway products)
{
    public async Task ExecuteAsync(string? tenantId, string productId, string status, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        if (!ProductStatuses.All.Contains(status)) throw AppException.Validation("Status inválido.");
        await products.SetStatusAsync(tenant.Id, productId, status, ct);
    }
}
