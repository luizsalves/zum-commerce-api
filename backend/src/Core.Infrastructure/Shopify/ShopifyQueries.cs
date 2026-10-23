namespace Core.Infrastructure.Shopify;

/// <summary>Operações GraphQL da Admin API usadas pela plataforma.</summary>
internal static class ShopifyQueries
{
    public const string Shop = """
        query { shop { name myshopifyDomain currencyCode plan { publicDisplayName } } }
        """;

    public const string PrimaryLocation = """
        query { locations(first: 1, query: "active:true") { nodes { id } } }
        """;

    public const string ListProducts = """
        query($first: Int!, $after: String, $query: String) {
          products(first: $first, after: $after, query: $query, sortKey: UPDATED_AT, reverse: true) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id title status totalInventory
              featuredMedia { preview { image { url } } }
              priceRangeV2 { minVariantPrice { amount } maxVariantPrice { amount } }
            }
          }
        }
        """;

    public const string GetProduct = """
        query($id: ID!) {
          product(id: $id) {
            id title descriptionHtml vendor productType tags status hasOnlyDefaultVariant
            options { name optionValues { name } }
            media(first: 20) { nodes { id alt ... on MediaImage { image { url } } } }
            variants(first: 100) {
              nodes {
                id price compareAtPrice inventoryQuantity
                selectedOptions { name value }
                inventoryItem { sku unitCost { amount } measurement { weight { unit value } } }
              }
            }
          }
        }
        """;

    public const string ProductSet = """
        mutation($input: ProductSetInput!, $identifier: ProductSetIdentifiers) {
          productSet(input: $input, identifier: $identifier, synchronous: true) {
            product { id }
            userErrors { field message }
          }
        }
        """;

    public const string FindCustomerByEmail = """
        query($query: String!) { customers(first: 1, query: $query) { nodes { id } } }
        """;

    public const string CreateCustomer = """
        mutation($input: CustomerInput!) {
          customerCreate(input: $input) {
            customer { id }
            userErrors { field message }
          }
        }
        """;

    public const string ListCustomers = """
        query($first: Int!, $after: String, $query: String) {
          customers(first: $first, after: $after, query: $query, sortKey: CREATED_AT, reverse: true) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id displayName createdAt numberOfOrders
              defaultEmailAddress { emailAddress }
              defaultPhoneNumber { phoneNumber }
              amountSpent { amount }
            }
          }
        }
        """;
}
