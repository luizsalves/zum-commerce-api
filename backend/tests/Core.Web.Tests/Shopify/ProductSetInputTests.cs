using Core.Domain.Catalog;
using Core.Domain.Constants;
using Core.Infrastructure.Shopify;

namespace Core.Web.Tests.Shopify;

public class ProductSetInputTests
{
    private const string Location = "gid://shopify/Location/1";

    [Fact]
    public void Simple_product_uses_shopify_default_option()
    {
        var product = new Product(null, "Laço", null, null, null, [], ProductStatuses.Active, [],
            [new ProductVariant(null, new Dictionary<string, string>(), 29.9m, null, 10m, "LACO-1", 5, 30m)], []);

        var input = ShopifyProductGateway.BuildInput(product, Location);

        var options = Assert.IsAssignableFrom<System.Collections.IList>(input["productOptions"]);
        var first = Assert.IsType<Dictionary<string, object?>>(options[0]);
        Assert.Equal(ShopifyDefaults.DefaultOptionName, first["name"]);

        var variants = Assert.IsAssignableFrom<System.Collections.IList>(input["variants"]);
        var variant = Assert.IsType<Dictionary<string, object?>>(variants[0]);
        Assert.Equal("29.90", variant["price"]);
        Assert.True(variant.ContainsKey("inventoryQuantities"));
    }

    [Fact]
    public void Existing_images_are_sent_by_id_and_new_ones_by_url()
    {
        var product = new Product("gid://shopify/Product/1", "Tiara", null, null, null, [], ProductStatuses.Active, [],
            [new ProductVariant("gid://shopify/ProductVariant/1", new Dictionary<string, string>(), 50m, null, null, null, null, null)],
            [new ProductImage("gid://shopify/MediaImage/9", "https://cdn/x.jpg", null), new ProductImage(null, "https://storage/nova.jpg", null)]);

        var input = ShopifyProductGateway.BuildInput(product, Location);
        var files = Assert.IsAssignableFrom<System.Collections.IList>(input["files"]);

        var existing = Assert.IsType<Dictionary<string, object?>>(files[0]);
        Assert.Equal("gid://shopify/MediaImage/9", existing["id"]);
        Assert.False(existing.ContainsKey("originalSource"));

        var added = Assert.IsType<Dictionary<string, object?>>(files[1]);
        Assert.Equal("https://storage/nova.jpg", added["originalSource"]);
    }

    [Fact]
    public void Quantity_omitted_when_not_informed()
    {
        var product = new Product(null, "Kit", null, null, null, [], ProductStatuses.Draft, [],
            [new ProductVariant(null, new Dictionary<string, string>(), 10m, null, null, null, null, null)], []);

        var variants = Assert.IsAssignableFrom<System.Collections.IList>(
            ShopifyProductGateway.BuildInput(product, Location)["variants"]);
        var variant = Assert.IsType<Dictionary<string, object?>>(variants[0]);
        Assert.False(variant.ContainsKey("inventoryQuantities"));
    }
}
