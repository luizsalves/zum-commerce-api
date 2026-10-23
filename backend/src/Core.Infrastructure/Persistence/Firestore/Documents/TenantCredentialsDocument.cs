using Core.Domain.Constants;
using Core.Domain.Tenants;
using Google.Cloud.Firestore;

namespace Core.Infrastructure.Persistence.Firestore.Documents;

/// <summary>tenantSecrets/{id}: o front nunca lê (bloqueado em firestore.rules).</summary>
[FirestoreData]
public sealed class TenantCredentialsDocument
{
    [FirestoreProperty("mode")] public string Mode { get; set; } = CredentialModes.ClientCredentials;
    [FirestoreProperty("clientId")] public string? ClientId { get; set; }
    [FirestoreProperty("clientSecretEnc")] public string? ClientSecretEncrypted { get; set; }
    [FirestoreProperty("accessTokenEnc")] public string? AccessTokenEncrypted { get; set; }
    [FirestoreProperty("updatedAt")] public Timestamp UpdatedAt { get; set; } = Timestamp.GetCurrentTimestamp();

    public TenantCredentials ToDomain() => new()
    {
        Mode = Mode,
        ClientId = ClientId,
        ClientSecretEncrypted = ClientSecretEncrypted,
        AccessTokenEncrypted = AccessTokenEncrypted,
        UpdatedAt = UpdatedAt.ToDateTimeOffset(),
    };

    public static TenantCredentialsDocument FromDomain(TenantCredentials c) => new()
    {
        Mode = c.Mode,
        ClientId = c.ClientId,
        ClientSecretEncrypted = c.ClientSecretEncrypted,
        AccessTokenEncrypted = c.AccessTokenEncrypted,
        UpdatedAt = Timestamp.FromDateTimeOffset(c.UpdatedAt),
    };
}
