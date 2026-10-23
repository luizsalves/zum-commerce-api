using Core.Application.Common;
using Core.Application.Exceptions;

namespace Core.Web.Tests.Tenants;

public class ShopDomainsTests
{
    [Theory]
    [InlineData("elika", "elika.myshopify.com")]
    [InlineData("https://Elika.myshopify.com/", "elika.myshopify.com")]
    public void Normalizes_shop_domain(string input, string expected) =>
        Assert.Equal(expected, ShopDomains.Normalize(input));

    [Fact]
    public void Rejects_custom_domain() =>
        Assert.Throws<AppException>(() => ShopDomains.Normalize("loja.elika.com.br"));
}
