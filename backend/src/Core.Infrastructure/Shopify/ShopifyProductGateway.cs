using System.Text.Json;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Domain.Catalog;
using Core.Domain.Common;
using Core.Domain.Constants;
using Core.Infrastructure.CrossCutting.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Infrastructure.Shopify;

/// <summary>
/// Produtos via productSet: cria/atualiza produto, opções, variações, preços, custo, peso, estoque e fotos numa chamada.
/// </summary>
public sealed class ShopifyProductGateway(ShopifyGraphQLClient client, IMemoryCache cache) : IShopifyProductGateway
{
    public async Task<PagedResult<ProductSummary>> ListAsync(string tenantId, string? search, string? after, CancellationToken ct)
    {
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.ListProducts, new { first = 30, after, query = search }, ct);
        var products = data.GetProperty("products");

        var items = products.GetProperty("nodes").EnumerateArray().Select(n => new ProductSummary(
            n.Str("id") ?? "",
            n.Str("title") ?? "",
            n.Str("status") ?? ProductStatuses.Draft,
            n.Int("totalInventory"),
            n.Str("featuredMedia", "preview", "image", "url"),
            n.Dec("priceRangeV2", "minVariantPrice", "amount"),
            n.Dec("priceRangeV2", "maxVariantPrice", "amount"))).ToList();

        return new PagedResult<ProductSummary>(
            items,
            products.Bool("pageInfo", "hasNextPage"),
            products.Str("pageInfo", "endCursor"));
    }

    public async Task<Product?> GetAsync(string tenantId, string productId, CancellationToken ct)
    {
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.GetProduct, new { id = productId }, ct);
        if (data.Walk("product") is not { } p) return null;

        var onlyDefault = p.Bool("hasOnlyDefaultVariant");

        var options = onlyDefault
            ? new List<ProductOption>()
            : p.GetProperty("options").EnumerateArray()
                .Select(o => new ProductOption(
                    o.Str("name") ?? "",
                    o.GetProperty("optionValues").EnumerateArray().Select(v => v.Str("name") ?? "").ToList()))
                .ToList();

        var variants = p.GetProperty("variants").GetProperty("nodes").EnumerateArray()
            .Select(v => new ProductVariant(
                v.Str("id"),
                onlyDefault
                    ? new Dictionary<string, string>()
                    : v.GetProperty("selectedOptions").EnumerateArray()
                        .ToDictionary(s => s.Str("name") ?? "", s => s.Str("value") ?? ""),
                v.Dec("price") ?? 0,
                v.Dec("compareAtPrice"),
                v.Dec("inventoryItem", "unitCost", "amount"),
                v.Str("inventoryItem", "sku"),
                v.Int("inventoryQuantity"),
                v.Walk("inventoryItem", "measurement", "weight") is { } w ? ToGrams(w) : null))
            .ToList();

        var images = p.GetProperty("media").GetProperty("nodes").EnumerateArray()
            .Where(m => m.Str("image", "url") is not null)
            .Select(m => new ProductImage(m.Str("id"), m.Str("image", "url"), m.Str("alt")))
            .ToList();

        return new Product(
            p.Str("id"),
            p.Str("title") ?? "",
            p.Str("descriptionHtml"),
            p.Str("vendor"),
            p.Str("productType"),
            p.GetProperty("tags").EnumerateArray().Select(t => t.GetString() ?? "").ToList(),
            p.Str("status") ?? ProductStatuses.Draft,
            options,
            variants,
            images);
    }

    public async Task<string> SaveAsync(string tenantId, Product product, CancellationToken ct)
    {
        var locationId = await GetPrimaryLocationIdAsync(tenantId, ct);
        var input = BuildInput(product, locationId);
        object? identifier = product.Id is null ? null : new { id = product.Id };

        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.ProductSet, new { input, identifier }, ct);
        var payload = data.GetProperty("productSet");
        ShopifyGraphQLClient.ThrowOnUserErrors(payload);
        return payload.Str("product", "id") ?? throw AppException.External("A Shopify não devolveu o produto salvo.");
    }

    public async Task SetStatusAsync(string tenantId, string productId, string status, CancellationToken ct)
    {
        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.ProductSet, new
        {
            input = new { status },
            identifier = new { id = productId },
        }, ct);
        ShopifyGraphQLClient.ThrowOnUserErrors(data.GetProperty("productSet"));
    }

    /// <summary>Local de estoque principal (o "local de processamento de pedidos" da loja).</summary>
    private async Task<string> GetPrimaryLocationIdAsync(string tenantId, CancellationToken ct)
    {
        var key = $"shopify-location:{tenantId}";
        if (cache.TryGetValue<string>(key, out var cached) && cached is not null) return cached;

        var data = await client.ExecuteAsync(tenantId, ShopifyQueries.PrimaryLocation, null, ct);
        var nodes = data.GetProperty("locations").GetProperty("nodes");
        var id = nodes.GetArrayLength() > 0 ? nodes[0].Str("id") : null;
        if (id is null) throw AppException.Unprocessable("A loja não tem nenhum local de estoque ativo na Shopify.");

        cache.Set(key, id, TimeSpan.FromHours(6));
        return id;
    }

    /// <summary>Monta o ProductSetInput. Visível para testes.</summary>
    internal static Dictionary<string, object?> BuildInput(Product product, string locationId)
    {
        var hasOptions = product.HasOptions;

        var productOptions = hasOptions
            ? product.Options.Select((o, i) => new Dictionary<string, object?>
            {
                ["name"] = o.Name.Trim(),
                ["position"] = i + 1,
                ["values"] = o.Values.Select(v => new { name = v.Trim() }).ToList(),
            }).ToList()
            : [new Dictionary<string, object?>
            {
                ["name"] = ShopifyDefaults.DefaultOptionName,
                ["values"] = new[] { new { name = ShopifyDefaults.DefaultOptionValue } },
            }];

        var variants = product.Variants.Select(v =>
        {
            var optionValues = hasOptions
                ? v.Options.Select(kv => new { optionName = kv.Key.Trim(), name = kv.Value.Trim() }).ToList()
                : [new { optionName = ShopifyDefaults.DefaultOptionName, name = ShopifyDefaults.DefaultOptionValue }];

            var inventoryItem = new Dictionary<string, object?> { ["tracked"] = true };
            if (!string.IsNullOrWhiteSpace(v.Sku)) inventoryItem["sku"] = v.Sku.Trim();
            if (v.Cost is { } cost) inventoryItem["cost"] = cost.ToShopifyMoney();
            if (v.WeightGrams is > 0m)
                inventoryItem["measurement"] = new { weight = new { unit = "GRAMS", value = v.WeightGrams.Value } };

            var variant = new Dictionary<string, object?>
            {
                ["optionValues"] = optionValues,
                ["price"] = v.Price.ToShopifyMoney(),
                ["inventoryItem"] = inventoryItem,
            };
            if (v.Id is not null) variant["id"] = v.Id;
            if (v.CompareAtPrice is > 0m) variant["compareAtPrice"] = v.CompareAtPrice.Value.ToShopifyMoney();
            if (v.Quantity is { } quantity)
            {
                variant["inventoryQuantities"] = new[]
                {
                    new { locationId, name = "available", quantity },
                };
            }
            return variant;
        }).ToList();

        // productSet trata "files" como lista completa: imagens não enviadas são removidas.
        // Por isso as existentes vão por id, junto com as novas por URL.
        var files = product.Images.Select(img => img.Id is not null
            ? new Dictionary<string, object?> { ["id"] = img.Id }
            : new Dictionary<string, object?>
            {
                ["originalSource"] = img.Url,
                ["contentType"] = "IMAGE",
                ["alt"] = img.Alt ?? product.Title,
            }).ToList();

        return new Dictionary<string, object?>
        {
            ["title"] = product.Title.Trim(),
            ["descriptionHtml"] = product.DescriptionHtml ?? "",
            ["vendor"] = product.Vendor,
            ["productType"] = product.ProductType,
            ["tags"] = product.Tags,
            ["status"] = product.Status,
            ["productOptions"] = productOptions,
            ["variants"] = variants,
            ["files"] = files,
        };
    }

    private static decimal? ToGrams(JsonElement weight)
    {
        var value = weight.Dec("value");
        if (value is null) return null;
        return weight.Str("unit") switch
        {
            "KILOGRAMS" => value * 1000,
            "POUNDS" => Math.Round(value.Value * 453.592m, 0),
            "OUNCES" => Math.Round(value.Value * 28.3495m, 0),
            _ => value,
        };
    }
}
