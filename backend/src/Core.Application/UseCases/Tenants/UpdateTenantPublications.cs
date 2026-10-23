using Core.Application.Contracts.Tenants;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;

namespace Core.Application.UseCases.Tenants;

/// <summary>Define em quais canais de vendas da Shopify os produtos da loja são publicados (ex.: Headless do site).</summary>
public sealed class UpdateTenantPublications(TenantAccessService access, ITenantRepository tenants)
{
    private const string PublicationPrefix = "gid://shopify/Publication/";

    public async Task ExecuteAsync(string tenantId, UpdatePublicationsRequest request, CancellationToken ct)
    {
        access.RequirePlatformAdmin();
        var tenant = await access.RequireTenantAsync(tenantId, ct);

        var ids = (request.PublicationIds ?? [])
            .Select(id => id.Trim())
            .Where(id => id.Length > 0)
            .Distinct()
            .ToList();
        if (ids.Any(id => !id.StartsWith(PublicationPrefix, StringComparison.Ordinal)))
            throw AppException.Validation("Canal de vendas inválido.");

        await tenants.UpdatePublicationsAsync(tenant.Id, ids, ct);
    }
}
