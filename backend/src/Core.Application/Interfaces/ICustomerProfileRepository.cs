using Core.Domain.Customers;

namespace Core.Application.Interfaces;

public interface ICustomerProfileRepository
{
    Task<StoreCustomer?> GetAsync(string tenantId, string uid, CancellationToken ct);
    Task SaveAsync(string tenantId, string uid, StoreCustomer customer, CancellationToken ct);
}
