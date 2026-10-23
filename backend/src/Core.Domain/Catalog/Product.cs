namespace Core.Domain.Catalog;

/// <summary>Opção do produto, ex.: Tamanho = [P, M, G]. Sem opções = produto simples.</summary>
public sealed record ProductOption(string Name, IReadOnlyList<string> Values);

/// <summary>Variação vendável. Options mapeia nome da opção → valor (ex.: { "Tamanho": "M" }).</summary>
public sealed record ProductVariant(
    string? Id,
    IReadOnlyDictionary<string, string> Options,
    decimal Price,
    decimal? CompareAtPrice,
    decimal? Cost,
    string? Sku,
    int? Quantity,
    decimal? WeightGrams);

/// <summary>Imagem: Id = mídia já existente na Shopify; Url = imagem nova (ex.: link do Firebase Storage).</summary>
public sealed record ProductImage(string? Id, string? Url, string? Alt);

public sealed record Product(
    string? Id,
    string Title,
    string? DescriptionHtml,
    string? Vendor,
    string? ProductType,
    IReadOnlyList<string> Tags,
    string Status,
    IReadOnlyList<ProductOption> Options,
    IReadOnlyList<ProductVariant> Variants,
    IReadOnlyList<ProductImage> Images)
{
    public bool HasOptions => Options.Count > 0;
}

/// <summary>Linha da listagem de produtos no painel.</summary>
public sealed record ProductSummary(
    string Id,
    string Title,
    string Status,
    int? TotalInventory,
    string? ImageUrl,
    decimal? MinPrice,
    decimal? MaxPrice);
