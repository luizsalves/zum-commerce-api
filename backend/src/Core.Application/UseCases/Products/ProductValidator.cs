using Core.Application.Exceptions;
using Core.Domain.Catalog;
using Core.Domain.Constants;

namespace Core.Application.UseCases.Products;

/// <summary>Regras de cadastro de produto, conferidas antes de mandar para a Shopify.</summary>
public static class ProductValidator
{
    public static void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Title)) throw AppException.Validation("Informe o nome do produto.");
        if (!ProductStatuses.All.Contains(product.Status)) throw AppException.Validation("Status inválido.");
        if (product.Variants.Count == 0) throw AppException.Validation("Informe ao menos um preço.");
        if (product.Options.Count > ShopifyDefaults.MaxOptions)
            throw AppException.Validation("A Shopify aceita no máximo 3 opções (ex.: Tamanho, Cor, Modelo).");
        if (!product.HasOptions && product.Variants.Count > 1)
            throw AppException.Validation("Produto sem opções deve ter um único preço.");

        foreach (var option in product.Options)
        {
            if (string.IsNullOrWhiteSpace(option.Name)) throw AppException.Validation("Toda opção precisa de um nome.");
            if (option.Values.Count == 0) throw AppException.Validation($"Informe os valores de \"{option.Name}\".");
        }

        var names = product.Options.Select(o => o.Name).ToList();
        if (names.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw AppException.Validation("Há opções com o mesmo nome.");

        var seen = new HashSet<string>();
        foreach (var variant in product.Variants)
        {
            if (variant.Price < 0) throw AppException.Validation("Preço não pode ser negativo.");
            if (variant.CompareAtPrice is < 0m || variant.Cost is < 0m) throw AppException.Validation("Valores não podem ser negativos.");
            if (variant.Quantity is < 0) throw AppException.Validation("Estoque não pode ser negativo.");

            foreach (var option in product.Options)
            {
                if (!variant.Options.TryGetValue(option.Name, out var value) || !option.Values.Contains(value))
                    throw AppException.Validation($"Variação sem valor válido para \"{option.Name}\".");
            }

            var key = string.Join("|", names.Select(n => variant.Options.GetValueOrDefault(n) ?? ""));
            if (!seen.Add(key)) throw AppException.Validation("Há variações repetidas.");
        }
    }
}
