using Google.Cloud.Firestore;

namespace Core.Infrastructure.Persistence.Firestore.Documents;

[FirestoreData]
public sealed class TenantMemberDocument
{
    [FirestoreProperty("email")] public string Email { get; set; } = "";
    [FirestoreProperty("role")] public string Role { get; set; } = "admin";
    [FirestoreProperty("addedAt")] public Timestamp AddedAt { get; set; } = Timestamp.GetCurrentTimestamp();
}
