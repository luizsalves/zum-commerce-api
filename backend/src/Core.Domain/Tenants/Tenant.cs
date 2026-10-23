namespace Core.Domain.Tenants;

/// <summary>Loja de um cliente da Zum (tenant). Contém só dados públicos da vitrine.</summary>
public sealed class Tenant
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string ShopDomain { get; set; }          // ex.: elika.myshopify.com
    public required string StorefrontToken { get; set; }     // token público da Storefront API
    public string ApiVersion { get; set; } = Constants.ShopifyDefaults.ApiVersion;
    public string? PrimaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? Whatsapp { get; set; }
    public List<string> Domains { get; set; } = [];
    public List<string> PublicationIds { get; set; } = [];   // canais de vendas que recebem os produtos (ex.: Headless)
    public bool Active { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
