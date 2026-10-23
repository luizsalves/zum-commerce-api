using Core.Domain.Customers;
using Google.Cloud.Firestore;

namespace Core.Infrastructure.Persistence.Firestore.Documents;

[FirestoreData]
public sealed class StoreCustomerDocument
{
    [FirestoreProperty("shopifyCustomerId")] public string ShopifyCustomerId { get; set; } = "";
    [FirestoreProperty("firstName")] public string FirstName { get; set; } = "";
    [FirestoreProperty("lastName")] public string LastName { get; set; } = "";
    [FirestoreProperty("email")] public string Email { get; set; } = "";
    [FirestoreProperty("phone")] public string? Phone { get; set; }
    [FirestoreProperty("acceptsMarketing")] public bool AcceptsMarketing { get; set; }
    [FirestoreProperty("createdAt")] public Timestamp CreatedAt { get; set; } = Timestamp.GetCurrentTimestamp();

    public StoreCustomer ToDomain() => new()
    {
        ShopifyCustomerId = ShopifyCustomerId,
        FirstName = FirstName,
        LastName = LastName,
        Email = Email,
        Phone = Phone,
        AcceptsMarketing = AcceptsMarketing,
        CreatedAt = CreatedAt.ToDateTimeOffset(),
    };

    public static StoreCustomerDocument FromDomain(StoreCustomer c) => new()
    {
        ShopifyCustomerId = c.ShopifyCustomerId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        Phone = c.Phone,
        AcceptsMarketing = c.AcceptsMarketing,
        CreatedAt = Timestamp.FromDateTimeOffset(c.CreatedAt),
    };
}
