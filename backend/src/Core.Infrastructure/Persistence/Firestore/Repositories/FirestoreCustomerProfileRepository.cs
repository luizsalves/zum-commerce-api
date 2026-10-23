using Core.Application.Interfaces;
using Core.Domain.Customers;
using Core.Infrastructure.Persistence.Firestore.Documents;
using Google.Cloud.Firestore;

namespace Core.Infrastructure.Persistence.Firestore.Repositories;

public sealed class FirestoreCustomerProfileRepository(FirestoreDb db) : ICustomerProfileRepository
{
    private DocumentReference Ref(string tenantId, string uid) => db
        .Collection(FirestoreCollections.Tenants).Document(tenantId)
        .Collection(FirestoreCollections.Customers).Document(uid);

    public async Task<StoreCustomer?> GetAsync(string tenantId, string uid, CancellationToken ct)
    {
        var snap = await Ref(tenantId, uid).GetSnapshotAsync(ct);
        return snap.Exists ? snap.ConvertTo<StoreCustomerDocument>().ToDomain() : null;
    }

    public Task SaveAsync(string tenantId, string uid, StoreCustomer customer, CancellationToken ct) =>
        Ref(tenantId, uid).SetAsync(StoreCustomerDocument.FromDomain(customer), cancellationToken: ct);
}
