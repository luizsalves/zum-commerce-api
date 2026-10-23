namespace Core.Infrastructure.Persistence.Firestore;

/// <summary>Nomes das coleções. As regras em firestore.rules (repositório do front) usam os mesmos nomes.</summary>
public static class FirestoreCollections
{
    public const string Tenants = "tenants";
    public const string TenantSecrets = "tenantSecrets";
    public const string Members = "members";          // tenants/{id}/members/{uid}
    public const string Customers = "customers";      // tenants/{id}/customers/{uid}
    public const string Users = "users";              // users/{uid}.tenants = [ids]
    public const string Domains = "domains";          // domains/{host}.tenantId
}
