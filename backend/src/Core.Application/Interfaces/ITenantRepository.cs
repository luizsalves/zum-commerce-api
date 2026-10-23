using Core.Domain.Tenants;

namespace Core.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetAsync(string tenantId, CancellationToken ct);
    Task<bool> ExistsAsync(string tenantId, CancellationToken ct);
    Task CreateAsync(Tenant tenant, TenantCredentials credentials, TenantMember owner, CancellationToken ct);
    Task<TenantCredentials?> GetCredentialsAsync(string tenantId, CancellationToken ct);
    Task SaveCredentialsAsync(string tenantId, TenantCredentials credentials, CancellationToken ct);
    Task UpdatePublicationsAsync(string tenantId, IReadOnlyList<string> publicationIds, CancellationToken ct);
    Task<bool> IsMemberAsync(string tenantId, string uid, CancellationToken ct);
    Task AddMemberAsync(TenantMember member, CancellationToken ct);
    Task<IReadOnlyList<Tenant>> ListAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Tenant>> ListForUserAsync(string uid, CancellationToken ct);
}
