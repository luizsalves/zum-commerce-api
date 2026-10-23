using Core.Application.Common;
using Core.Application.Contracts.Tenants;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Constants;
using Core.Domain.Tenants;
using Core.Infrastructure.CrossCutting.Extensions;

namespace Core.Application.UseCases.Tenants;

/// <summary>Cadastra a loja de um cliente. Quem cadastra já entra como dono.</summary>
public sealed class CreateTenant(
    TenantAccessService access,
    ITenantRepository tenants,
    ISecretProtector protector,
    ICurrentUser user)
{
    public async Task<string> ExecuteAsync(CreateTenantRequest request, CancellationToken ct)
    {
        access.RequirePlatformAdmin();

        var id = request.Id.Trim().ToLowerInvariant();
        if (!TenantIds.IsValid(id))
            throw AppException.Validation("ID inválido. Use letras minúsculas, números e hífen (ex.: elika).");
        if (string.IsNullOrWhiteSpace(request.Name)) throw AppException.Validation("Informe o nome da loja.");
        if (string.IsNullOrWhiteSpace(request.StorefrontToken)) throw AppException.Validation("Informe o token da Storefront API.");
        if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.ClientSecret))
            throw AppException.Validation("Informe o Client ID e o Client Secret.");
        if (await tenants.ExistsAsync(id, ct)) throw AppException.Conflict("Já existe uma loja com esse ID.");

        var tenant = new Tenant
        {
            Id = id,
            Name = request.Name.Trim(),
            ShopDomain = ShopDomains.Normalize(request.ShopDomain),
            StorefrontToken = request.StorefrontToken.Trim(),
            PrimaryColor = request.PrimaryColor.TrimToNull(),
            Whatsapp = request.Whatsapp.TrimToNull(),
            Domains = (request.Domains ?? [])
                .Select(d => d.Trim().ToLowerInvariant())
                .Where(d => d.Length > 0)
                .Distinct()
                .ToList(),
        };

        var credentials = new TenantCredentials
        {
            Mode = CredentialModes.ClientCredentials,
            ClientId = request.ClientId.Trim(),
            ClientSecretEncrypted = protector.Protect(request.ClientSecret.Trim()),
        };

        var owner = new TenantMember(id, access.RequireUid(), user.Email ?? "", MemberRoles.Owner);
        await tenants.CreateAsync(tenant, credentials, owner, ct);
        return id;
    }
}
