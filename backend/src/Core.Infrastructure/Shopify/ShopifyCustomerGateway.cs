using System.Text.Json;
using Core.Application.Contracts.Customers;
using Core.Application.Interfaces;
using Core.Domain.Common;
using Core.Domain.Customers;

namespace Core.Infrastructure.Shopify;

public sealed class ShopifyCustomerGateway(ShopifyGraphQLClient client) : IShopifyCustomerGateway
{
    public async Task<string?> FindIdByEmailAsync(string tenantId, string email, CancellationToken ct)
    {
        var search = $"email:\"{email.Replace("\"", "")}\"";
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.FindCustomerByEmail, new { query = search }, ct);
        var nodes = data.GetProperty("customers").GetProperty("nodes");
        return nodes.GetArrayLength() > 0 ? nodes[0].Str("id") : null;
    }

    public async Task<string> CreateAsync(string tenantId, NewShopifyCustomer customer, CancellationToken ct)
    {
        var input = new Dictionary<string, object?>
        {
            ["firstName"] = customer.FirstName,
            ["lastName"] = customer.LastName,
            ["email"] = customer.Email,
            ["tags"] = new[] { "cadastro-site" },
        };
        if (customer.PhoneE164 is not null) input["phone"] = customer.PhoneE164;
        if (customer.AcceptsMarketing)
        {
            input["emailMarketingConsent"] = new
            {
                marketingState = "SUBSCRIBED",
                marketingOptInLevel = "SINGLE_OPT_IN",
            };
        }

        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.CreateCustomer, new { input }, ct);
        var payload = data.GetProperty("customerCreate");
        ShopifyGraphQLClient.ThrowOnUserErrors(payload);
        return payload.Str("customer", "id")
            ?? throw Core.Application.Exceptions.AppException.External("A Shopify não devolveu o cliente criado.");
    }

    public async Task<PagedResult<CustomerSummary>> ListAsync(string tenantId, string? search, string? after, CancellationToken ct)
    {
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.ListCustomers, new { first = 50, after, query = search }, ct);
        var customers = data.GetProperty("customers");

        var items = customers.GetProperty("nodes").EnumerateArray().Select(Map).ToList();
        return new PagedResult<CustomerSummary>(
            items,
            customers.Bool("pageInfo", "hasNextPage"),
            customers.Str("pageInfo", "endCursor"));
    }

    private static CustomerSummary Map(JsonElement n) => new(
        n.Str("id") ?? "",
        n.Str("displayName") ?? "",
        n.Str("defaultEmailAddress", "emailAddress"),
        n.Str("defaultPhoneNumber", "phoneNumber"),
        n.Long("numberOfOrders"),
        n.Dec("amountSpent", "amount") ?? 0,
        n.Str("createdAt"));
}
