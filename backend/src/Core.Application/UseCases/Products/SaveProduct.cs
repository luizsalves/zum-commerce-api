using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Catalog;

namespace Core.Application.UseCases.Products;

/// <summary>Cria ou atualiza o produto na Shopify e devolve como ficou salvo.</summary>
public sealed class SaveProduct(TenantAccessService access, IShopifyProductGateway products)
{
    public async Task<Product> ExecuteAsync(string? tenantId, Product product, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        ProductValidator.Validate(product);

        var id = await products.SaveAsync(tenant.Id, product, ct);
        return await products.GetAsync(tenant.Id, id, ct)
            ?? throw AppException.External("O produto foi salvo, mas não foi possível relê-lo na Shopify.");
    }
}
