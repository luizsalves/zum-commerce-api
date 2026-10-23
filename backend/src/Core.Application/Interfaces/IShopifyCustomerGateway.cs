using Core.Application.Contracts.Customers;
using Core.Domain.Common;
using Core.Domain.Customers;

namespace Core.Application.Interfaces;

/// <summary>Clientes na Shopify (Admin API).</summary>
public interface IShopifyCustomerGateway
{
    Task<string?> FindIdByEmailAsync(string tenantId, string email, CancellationToken ct);
    Task<string> CreateAsync(string tenantId, NewShopifyCustomer customer, CancellationToken ct);
    Task<PagedResult<CustomerSummary>> ListAsync(string tenantId, string? search, string? after, CancellationToken ct);
}
