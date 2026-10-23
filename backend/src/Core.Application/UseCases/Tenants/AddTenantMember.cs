using Core.Application.Contracts.Tenants;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Constants;
using Core.Domain.Tenants;
using Core.Infrastructure.CrossCutting.Extensions;

namespace Core.Application.UseCases.Tenants;

/// <summary>Libera o acesso de alguém ao painel de uma loja.</summary>
public sealed class AddTenantMember(TenantAccessService access, ITenantRepository tenants, IUserDirectory directory)
{
    public async Task<MemberResponse> ExecuteAsync(string tenantId, AddMemberRequest request, CancellationToken ct)
    {
        access.RequirePlatformAdmin();
        var tenant = await access.RequireTenantAsync(tenantId, ct);

        if (string.IsNullOrWhiteSpace(request.Email)) throw AppException.Validation("Informe o e-mail.");
        var user = await directory.FindByEmailAsync(request.Email.NormalizeEmail(), ct)
            ?? throw AppException.NotFound("Essa pessoa ainda não tem conta. Peça para ela entrar uma vez no painel com esse e-mail.");

        var role = MemberRoles.Normalize(request.Role);
        await tenants.AddMemberAsync(new TenantMember(tenant.Id, user.Uid, user.Email ?? request.Email, role), ct);
        return new MemberResponse(user.Uid, user.Email, role);
    }
}
