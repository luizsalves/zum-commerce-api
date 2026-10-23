using Core.Application.Common;
using Core.Application.Configuration;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Domain.Tenants;

namespace Core.Application.Services;

/// <summary>Regras de acesso: quem é da equipe da plataforma e quem administra cada loja.</summary>
public sealed class TenantAccessService(ITenantRepository tenants, ICurrentUser user, PlatformOptions platform)
{
    public bool IsPlatformAdmin =>
        user.IsAuthenticated
        && user.EmailVerified
        && user.Email is { } email
        && platform.AdminEmailSet.Contains(email.ToLowerInvariant());

    public string RequireUid() =>
        user.IsAuthenticated && !string.IsNullOrEmpty(user.Uid) ? user.Uid! : throw AppException.Unauthorized();

    public void RequirePlatformAdmin()
    {
        RequireUid();
        if (!IsPlatformAdmin) throw AppException.Forbidden("Apenas a equipe da plataforma pode fazer isso.");
    }

    /// <summary>Confirma que a loja existe e está ativa.</summary>
    public async Task<Tenant> RequireTenantAsync(string? tenantId, CancellationToken ct)
    {
        if (!TenantIds.IsValid(tenantId))
            throw AppException.Validation($"Envie o cabeçalho {TenantIds.HeaderName} com o ID da loja.");

        var tenant = await tenants.GetAsync(tenantId!, ct);
        if (tenant is null || !tenant.Active) throw AppException.NotFound("Loja não encontrada.");
        return tenant;
    }

    /// <summary>Exige que o usuário administre a loja (ou seja da equipe da plataforma).</summary>
    public async Task<Tenant> RequireMemberAsync(string? tenantId, CancellationToken ct)
    {
        var uid = RequireUid();
        var tenant = await RequireTenantAsync(tenantId, ct);
        if (IsPlatformAdmin) return tenant;
        if (!await tenants.IsMemberAsync(tenant.Id, uid, ct)) throw AppException.Forbidden();
        return tenant;
    }
}
