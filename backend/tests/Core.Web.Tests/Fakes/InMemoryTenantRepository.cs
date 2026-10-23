using Core.Application.Interfaces;
using Core.Domain.Tenants;

namespace Core.Web.Tests.Fakes;

public sealed class InMemoryTenantRepository : ITenantRepository
{
    public Dictionary<string, Tenant> Tenants { get; } = new();
    public Dictionary<string, TenantCredentials> Credentials { get; } = new();
    public List<TenantMember> Members { get; } = new();

    public void Seed(string id, bool active = true) => Tenants[id] = new Tenant
    {
        Id = id,
        Name = id,
        ShopDomain = $"{id}.myshopify.com",
        StorefrontToken = "public-token",
        Active = active,
    };

    public Task<Tenant?> GetAsync(string tenantId, CancellationToken ct) =>
        Task.FromResult(Tenants.GetValueOrDefault(tenantId));

    public Task<bool> ExistsAsync(string tenantId, CancellationToken ct) => Task.FromResult(Tenants.ContainsKey(tenantId));

    public Task CreateAsync(Tenant tenant, TenantCredentials credentials, TenantMember owner, CancellationToken ct)
    {
        Tenants[tenant.Id] = tenant;
        Credentials[tenant.Id] = credentials;
        Members.Add(owner);
        return Task.CompletedTask;
    }

    public Task<TenantCredentials?> GetCredentialsAsync(string tenantId, CancellationToken ct) =>
        Task.FromResult(Credentials.GetValueOrDefault(tenantId));

    public Task SaveCredentialsAsync(string tenantId, TenantCredentials credentials, CancellationToken ct)
    {
        Credentials[tenantId] = credentials;
        return Task.CompletedTask;
    }

    public Task UpdatePublicationsAsync(string tenantId, IReadOnlyList<string> publicationIds, CancellationToken ct)
    {
        Tenants[tenantId].PublicationIds = publicationIds.ToList();
        return Task.CompletedTask;
    }

    public Task<bool> IsMemberAsync(string tenantId, string uid, CancellationToken ct) =>
        Task.FromResult(Members.Any(m => m.TenantId == tenantId && m.Uid == uid));

    public Task AddMemberAsync(TenantMember member, CancellationToken ct)
    {
        Members.Add(member);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Tenant>> ListAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Tenant>>(Tenants.Values.ToList());

    public Task<IReadOnlyList<Tenant>> ListForUserAsync(string uid, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Tenant>>(
            Members.Where(m => m.Uid == uid).Select(m => Tenants[m.TenantId]).ToList());
}
