using Core.Application.Contracts.Tenants;
using Core.Application.Interfaces;
using Core.Application.Services;

namespace Core.Application.UseCases.Tenants;

/// <summary>Quem sou eu no painel: equipe da plataforma? Quais lojas administro?</summary>
public sealed class GetMe(TenantAccessService access, ITenantRepository tenants)
{
    public async Task<MeResponse> ExecuteAsync(CancellationToken ct)
    {
        var uid = access.RequireUid();
        var list = access.IsPlatformAdmin
            ? await tenants.ListAllAsync(ct)
            : await tenants.ListForUserAsync(uid, ct);

        return new MeResponse(
            access.IsPlatformAdmin,
            list.Select(t => new TenantSummaryResponse(t.Id, t.Name, t.ShopDomain, t.Active)).ToList());
    }
}
