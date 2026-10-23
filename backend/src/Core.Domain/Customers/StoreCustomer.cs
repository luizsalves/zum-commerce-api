namespace Core.Domain.Customers;

/// <summary>Cliente final cadastrado na vitrine, vinculado a um Customer da Shopify.</summary>
public sealed class StoreCustomer
{
    public required string ShopifyCustomerId { get; init; }
    public required string FirstName { get; init; }
    public string LastName { get; init; } = "";
    public required string Email { get; init; }
    public string? Phone { get; init; }
    public bool AcceptsMarketing { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Linha da listagem de clientes no painel (dados vindos da Shopify).</summary>
public sealed record CustomerSummary(
    string Id,
    string Name,
    string? Email,
    string? Phone,
    long OrdersCount,
    decimal AmountSpent,
    string? CreatedAt);
