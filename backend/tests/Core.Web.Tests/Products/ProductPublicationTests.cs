using Core.Application.Configuration;
using Core.Application.Contracts.Tenants;
using Core.Application.Exceptions;
using Core.Application.Services;
using Core.Application.UseCases.Products;
using Core.Application.UseCases.Tenants;
using Core.Domain.Catalog;
using Core.Domain.Constants;
using Core.Domain.Tenants;
using Core.Web.Tests.Fakes;

namespace Core.Web.Tests.Products;

public class ProductPublicationTests
{
    private const string Headless = "gid://shopify/Publication/123";

    private readonly InMemoryTenantRepository _repo = new();
    private readonly FakeCurrentUser _user = new();
    private readonly FakeProductGateway _products = new();
    private readonly PlatformOptions _platform = new() { AdminEmails = "dono@zum.com.br" };

    public ProductPublicationTests()
    {
        _repo.Seed("elika");
        _repo.Tenants["elika"].PublicationIds = [Headless];
        _repo.Members.Add(new TenantMember("elika", "user-1", "lojista@exemplo.com", MemberRoles.Owner));
    }

    private TenantAccessService Access() => new(_repo, _user, _platform);

    private static Product NewProduct(string status) => new(
        null, "Tiara", null, null, null, [], status, [],
        [new ProductVariant(null, new Dictionary<string, string>(), 79.90m, null, null, null, 5, null)],
        []);

    [Theory]
    [InlineData(ProductStatuses.Active, true)]
    [InlineData(ProductStatuses.Draft, false)]
    [InlineData(ProductStatuses.Archived, false)]
    public async Task Save_publishes_only_active_products(string status, bool published)
    {
        await new SaveProduct(Access(), _products).ExecuteAsync("elika", NewProduct(status), CancellationToken.None);

        var sync = Assert.Single(_products.Syncs);
        Assert.Equal(published, sync.Published);
        Assert.Equal([Headless], sync.PublicationIds);
    }

    [Fact]
    public async Task Status_change_updates_publication()
    {
        await new SetProductStatus(Access(), _products)
            .ExecuteAsync("elika", "gid://shopify/Product/9", ProductStatuses.Draft, CancellationToken.None);

        var sync = Assert.Single(_products.Syncs);
        Assert.Equal("gid://shopify/Product/9", sync.ProductId);
        Assert.False(sync.Published);
    }

    [Fact]
    public async Task Platform_admin_saves_publications()
    {
        _user.Email = "dono@zum.com.br";
        var request = new UpdatePublicationsRequest([" gid://shopify/Publication/7 ", "gid://shopify/Publication/7", ""]);

        await new UpdateTenantPublications(Access(), _repo).ExecuteAsync("elika", request, CancellationToken.None);

        Assert.Equal(["gid://shopify/Publication/7"], _repo.Tenants["elika"].PublicationIds);
    }

    [Fact]
    public async Task Invalid_publication_id_is_rejected()
    {
        _user.Email = "dono@zum.com.br";
        var ex = await Assert.ThrowsAsync<AppException>(() => new UpdateTenantPublications(Access(), _repo)
            .ExecuteAsync("elika", new UpdatePublicationsRequest(["gid://shopify/Product/1"]), CancellationToken.None));
        Assert.Equal(AppErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public async Task Store_member_cannot_change_publications()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => new UpdateTenantPublications(Access(), _repo)
            .ExecuteAsync("elika", new UpdatePublicationsRequest([Headless]), CancellationToken.None));
        Assert.Equal(AppErrorKind.Forbidden, ex.Kind);
    }
}
