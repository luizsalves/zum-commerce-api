using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Common;
using Core.Domain.Customers;

namespace Core.Application.UseCases.Customers;

public sealed class ListCustomers(TenantAccessService access, IShopifyCustomerGateway customers)
{
    public async Task<PagedResult<CustomerSummary>> ExecuteAsync(string? tenantId, string? search, string? after, CancellationToken ct)
    {
        var tenant = await access.RequireMemberAsync(tenantId, ct);
        return await customers.ListAsync(tenant.Id, string.IsNullOrWhiteSpace(search) ? null : search.Trim(), after, ct);
    }
}
