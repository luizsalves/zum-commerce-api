using Core.Application.Common;
using Core.Application.Contracts.Products;
using Core.Application.Interfaces;
using Core.Application.Services;

namespace Core.Application.UseCases.Products;

/// <summary>Recebe uma foto do painel e envia direto para a Shopify, sem passar pelo Firebase Storage.</summary>
public sealed class UploadProductImage(TenantAccessService access, IShopifyProductGateway products)
{
    public async Task<UploadedImageResponse> ExecuteAsync(
        string? tenantId, string? fileName, string? contentType, long length, Stream content, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        var type = ImageFiles.Validate(contentType, length);

        var url = await products.StageImageAsync(tenant.Id, ImageFiles.SafeName(fileName, type), type, content, ct);
        return new UploadedImageResponse(url);
    }
}
