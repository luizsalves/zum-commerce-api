namespace Core.Application.Contracts.Customers;

public sealed record RegisterCustomerRequest(string FirstName, string? LastName, string? Phone, bool AcceptsMarketing);

/// <summary>Dados para criar o Customer na Shopify.</summary>
public sealed record NewShopifyCustomer(
    string FirstName,
    string? LastName,
    string Email,
    string? PhoneE164,
    bool AcceptsMarketing);
