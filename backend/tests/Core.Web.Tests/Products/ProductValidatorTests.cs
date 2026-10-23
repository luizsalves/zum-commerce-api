using Core.Application.Exceptions;
using Core.Application.UseCases.Products;
using Core.Domain.Catalog;
using Core.Domain.Constants;

namespace Core.Web.Tests.Products;

public class ProductValidatorTests
{
    private static ProductVariant Variant(decimal price, params (string Name, string Value)[] options) => new(
        null, options.ToDictionary(o => o.Name, o => o.Value), price, null, null, null, 1, null);

    private static Product MakeProduct(IReadOnlyList<ProductOption> options, params ProductVariant[] variants) => new(
        null, "Tiara de pérolas", null, null, null, [], ProductStatuses.Active, options, variants, []);

    [Fact]
    public void Simple_product_is_valid()
    {
        ProductValidator.Validate(MakeProduct([], Variant(49.90m)));
    }

    [Fact]
    public void Product_with_sizes_is_valid()
    {
        var options = new[] { new ProductOption("Tamanho", ["P", "M"]) };
        ProductValidator.Validate(MakeProduct(options, Variant(79.90m, ("Tamanho", "P")), Variant(79.90m, ("Tamanho", "M"))));
    }

    [Fact]
    public void Variant_with_unknown_value_is_rejected()
    {
        var options = new[] { new ProductOption("Tamanho", ["P", "M"]) };
        var ex = Assert.Throws<AppException>(() => ProductValidator.Validate(MakeProduct(options, Variant(10m, ("Tamanho", "G")))));
        Assert.Equal(AppErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public void Duplicated_variants_are_rejected()
    {
        var options = new[] { new ProductOption("Cor", ["Rosa"]) };
        Assert.Throws<AppException>(() =>
            ProductValidator.Validate(MakeProduct(options, Variant(10m, ("Cor", "Rosa")), Variant(12m, ("Cor", "Rosa")))));
    }

    [Fact]
    public void More_than_three_options_are_rejected()
    {
        var options = new[]
        {
            new ProductOption("A", ["1"]), new ProductOption("B", ["1"]),
            new ProductOption("C", ["1"]), new ProductOption("D", ["1"]),
        };
        Assert.Throws<AppException>(() =>
            ProductValidator.Validate(MakeProduct(options, Variant(10m, ("A", "1"), ("B", "1"), ("C", "1"), ("D", "1")))));
    }

    [Fact]
    public void Negative_price_is_rejected()
    {
        Assert.Throws<AppException>(() => ProductValidator.Validate(MakeProduct([], Variant(-1m))));
    }
}
