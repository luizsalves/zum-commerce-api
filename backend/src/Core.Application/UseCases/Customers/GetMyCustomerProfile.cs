using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Customers;

namespace Core.Application.UseCases.Customers;

/// <summary>Cadastro do cliente logado nesta loja (null = ainda não completou o cadastro).</summary>
public sealed class GetMyCustomerProfile(TenantAccessService access, ICustomerProfileRepository profiles)
{
    public async Task<StoreCustomer?> ExecuteAsync(string? tenantId, CancellationToken ct)
    {
        var uid = access.RequireUid();
        var tenant = await access.RequireTenantAsync(tenantId, ct);
        return await profiles.GetAsync(tenant.Id, uid, ct);
    }
}
