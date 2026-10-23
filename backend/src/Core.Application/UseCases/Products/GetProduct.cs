using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Catalog;

namespace Core.Application.UseCases.Products;

public sealed class GetProduct(TenantAccessService access, IShopifyProductGateway products)
{
    public async Task<Product> ExecuteAsync(string? tenantId, string productId, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        if (string.IsNullOrWhiteSpace(productId)) throw AppException.Validation("Informe o produto.");
        return await products.GetAsync(tenant.Id, productId, ct) ?? throw AppException.NotFound("Produto não encontrado.");
    }
}
