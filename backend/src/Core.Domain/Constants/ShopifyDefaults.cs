namespace Core.Domain.Constants;

public static class ShopifyDefaults
{
    public const string ApiVersion = "2026-10";

    /// <summary>Opção que a Shopify usa em produtos sem variação.</summary>
    public const string DefaultOptionName = "Title";
    public const string DefaultOptionValue = "Default Title";

    public const int MaxOptions = 3;
}
