using Core.Application.Interfaces;
using Core.Domain.Tenants;
using Core.Infrastructure.Persistence.Firestore.Documents;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Infrastructure.Persistence.Firestore.Repositories;

public sealed class FirestoreTenantRepository(FirestoreDb db, IMemoryCache cache) : ITenantRepository
{
    private static readonly TimeSpan TenantCacheTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MemberCacheTtl = TimeSpan.FromMinutes(2);

    private DocumentReference TenantRef(string id) => db.Collection(FirestoreCollections.Tenants).Document(id);
    private DocumentReference SecretsRef(string id) => db.Collection(FirestoreCollections.TenantSecrets).Document(id);
    private DocumentReference MemberRef(string tenantId, string uid) =>
        TenantRef(tenantId).Collection(FirestoreCollections.Members).Document(uid);
    private DocumentReference UserRef(string uid) => db.Collection(FirestoreCollections.Users).Document(uid);
    private DocumentReference DomainRef(string host) => db.Collection(FirestoreCollections.Domains).Document(host);

    public async Task<Tenant?> GetAsync(string tenantId, CancellationToken ct)
    {
        if (cache.TryGetValue<Tenant>(TenantKey(tenantId), out var cached) && cached is not null) return cached;

        var snap = await TenantRef(tenantId).GetSnapshotAsync(ct);
        if (!snap.Exists) return null; // não guarda "não existe" em cache: a loja pode ser criada logo em seguida

        var tenant = snap.ConvertTo<TenantDocument>().ToDomain(snap.Id);
        cache.Set(TenantKey(tenantId), tenant, TenantCacheTtl);
        return tenant;
    }

    public async Task<bool> ExistsAsync(string tenantId, CancellationToken ct) =>
        (await TenantRef(tenantId).GetSnapshotAsync(ct)).Exists;

    public async Task CreateAsync(Tenant tenant, TenantCredentials credentials, TenantMember owner, CancellationToken ct)
    {
        var batch = db.StartBatch();
        batch.Create(TenantRef(tenant.Id), TenantDocument.FromDomain(tenant));
        batch.Set(SecretsRef(tenant.Id), TenantCredentialsDocument.FromDomain(credentials));
        foreach (var host in tenant.Domains)
            batch.Set(DomainRef(host), new Dictionary<string, object> { ["tenantId"] = tenant.Id });
        AddMemberWrites(batch, owner);
        await batch.CommitAsync(ct);
        cache.Remove(TenantKey(tenant.Id));
    }

    public async Task<TenantCredentials?> GetCredentialsAsync(string tenantId, CancellationToken ct)
    {
        var snap = await SecretsRef(tenantId).GetSnapshotAsync(ct);
        return snap.Exists ? snap.ConvertTo<TenantCredentialsDocument>().ToDomain() : null;
    }

    public Task SaveCredentialsAsync(string tenantId, TenantCredentials credentials, CancellationToken ct) =>
        SecretsRef(tenantId).SetAsync(TenantCredentialsDocument.FromDomain(credentials), cancellationToken: ct);

    public async Task<bool> IsMemberAsync(string tenantId, string uid, CancellationToken ct)
    {
        var key = $"member:{tenantId}:{uid}";
        if (cache.TryGetValue<bool>(key, out var cached)) return cached;
        var exists = (await MemberRef(tenantId, uid).GetSnapshotAsync(ct)).Exists;
        cache.Set(key, exists, MemberCacheTtl);
        return exists;
    }

    public async Task AddMemberAsync(TenantMember member, CancellationToken ct)
    {
        var batch = db.StartBatch();
        AddMemberWrites(batch, member);
        await batch.CommitAsync(ct);
        cache.Remove($"member:{member.TenantId}:{member.Uid}");
    }

    public async Task<IReadOnlyList<Tenant>> ListAllAsync(CancellationToken ct)
    {
        var snaps = await db.Collection(FirestoreCollections.Tenants).GetSnapshotAsync(ct);
        return snaps.Documents
            .Select(d => d.ConvertTo<TenantDocument>().ToDomain(d.Id))
            .OrderBy(t => t.Name)
            .ToList();
    }

    public async Task<IReadOnlyList<Tenant>> ListForUserAsync(string uid, CancellationToken ct)
    {
        var userSnap = await UserRef(uid).GetSnapshotAsync(ct);
        if (!userSnap.Exists || !userSnap.TryGetValue<List<string>>("tenants", out var ids)) return [];

        var result = new List<Tenant>();
        foreach (var id in ids)
        {
            var tenant = await GetAsync(id, ct);
            if (tenant is not null) result.Add(tenant);
        }
        return result.OrderBy(t => t.Name).ToList();
    }

    /// <summary>Grava o membro e o índice users/{uid}.tenants usado para listar as lojas de cada pessoa.</summary>
    private void AddMemberWrites(WriteBatch batch, TenantMember member)
    {
        batch.Set(MemberRef(member.TenantId, member.Uid), new TenantMemberDocument { Email = member.Email, Role = member.Role });
        batch.Set(UserRef(member.Uid),
            new Dictionary<string, object> { ["tenants"] = FieldValue.ArrayUnion(member.TenantId) },
            SetOptions.MergeAll);
    }

    private static string TenantKey(string id) => $"tenant:{id}";
}
