using Core.Application.Contracts.Tenants;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Constants;
using Core.Domain.Tenants;

namespace Core.Application.UseCases.Tenants;

public sealed class UpdateTenantCredentials(
    TenantAccessService access,
    ITenantRepository tenants,
    ISecretProtector protector,
    IShopifyShopGateway shop)
{
    public async Task ExecuteAsync(string tenantId, UpdateCredentialsRequest request, CancellationToken ct)
    {
        access.RequirePlatformAdmin();
        var tenant = await access.RequireTenantAsync(tenantId, ct);

        TenantCredentials credentials;
        if (!string.IsNullOrWhiteSpace(request.AccessToken))
        {
            credentials = new TenantCredentials
            {
                Mode = CredentialModes.AccessToken,
                AccessTokenEncrypted = protector.Protect(request.AccessToken.Trim()),
            };
        }
        else if (!string.IsNullOrWhiteSpace(request.ClientId) && !string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            credentials = new TenantCredentials
            {
                Mode = CredentialModes.ClientCredentials,
                ClientId = request.ClientId.Trim(),
                ClientSecretEncrypted = protector.Protect(request.ClientSecret.Trim()),
            };
        }
        else
        {
            throw AppException.Validation("Envie Client ID + Secret, ou um Access Token.");
        }

        await tenants.SaveCredentialsAsync(tenant.Id, credentials, ct);
        shop.InvalidateToken(tenant.Id);
    }
}
