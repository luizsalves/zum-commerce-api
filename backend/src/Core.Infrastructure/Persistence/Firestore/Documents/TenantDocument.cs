using Core.Domain.Constants;
using Core.Domain.Tenants;
using Google.Cloud.Firestore;

namespace Core.Infrastructure.Persistence.Firestore.Documents;

[FirestoreData]
public sealed class TenantDocument
{
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("shopDomain")] public string ShopDomain { get; set; } = "";
    [FirestoreProperty("storefrontToken")] public string StorefrontToken { get; set; } = "";
    [FirestoreProperty("apiVersion")] public string ApiVersion { get; set; } = ShopifyDefaults.ApiVersion;
    [FirestoreProperty("primaryColor")] public string? PrimaryColor { get; set; }
    [FirestoreProperty("logoUrl")] public string? LogoUrl { get; set; }
    [FirestoreProperty("whatsapp")] public string? Whatsapp { get; set; }
    [FirestoreProperty("domains")] public List<string> Domains { get; set; } = [];
    [FirestoreProperty("publicationIds")] public List<string> PublicationIds { get; set; } = [];
    [FirestoreProperty("active")] public bool Active { get; set; } = true;
    [FirestoreProperty("createdAt")] public Timestamp CreatedAt { get; set; } = Timestamp.GetCurrentTimestamp();

    public Tenant ToDomain(string id) => new()
    {
        Id = id,
        Name = Name,
        ShopDomain = ShopDomain,
        StorefrontToken = StorefrontToken,
        ApiVersion = string.IsNullOrWhiteSpace(ApiVersion) ? ShopifyDefaults.ApiVersion : ApiVersion,
        PrimaryColor = PrimaryColor,
        LogoUrl = LogoUrl,
        Whatsapp = Whatsapp,
        Domains = Domains ?? [],
        PublicationIds = PublicationIds ?? [],
        Active = Active,
        CreatedAt = CreatedAt.ToDateTimeOffset(),
    };

    public static TenantDocument FromDomain(Tenant t) => new()
    {
        Name = t.Name,
        ShopDomain = t.ShopDomain,
        StorefrontToken = t.StorefrontToken,
        ApiVersion = t.ApiVersion,
        PrimaryColor = t.PrimaryColor,
        LogoUrl = t.LogoUrl,
        Whatsapp = t.Whatsapp,
        Domains = t.Domains,
        PublicationIds = t.PublicationIds,
        Active = t.Active,
        CreatedAt = Timestamp.FromDateTimeOffset(t.CreatedAt),
    };
}
