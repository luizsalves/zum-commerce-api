using Core.Application.Common;
using Core.Application.Configuration;
using Core.Application.Exceptions;
using Core.Application.Services;
using Core.Application.UseCases.Products;
using Core.Domain.Constants;
using Core.Domain.Tenants;
using Core.Web.Tests.Fakes;

namespace Core.Web.Tests.Products;

public class ProductImageUploadTests
{
    private readonly InMemoryTenantRepository _repo = new();
    private readonly FakeCurrentUser _user = new();
    private readonly FakeProductGateway _products = new();
    private readonly PlatformOptions _platform = new() { AdminEmails = "dono@zum.com.br" };

    public ProductImageUploadTests()
    {
        _repo.Seed("elika");
        _repo.Members.Add(new TenantMember("elika", "user-1", "lojista@exemplo.com", MemberRoles.Owner));
    }

    private Task<Core.Application.Contracts.Products.UploadedImageResponse> Upload(string? fileName, string? contentType, long length) =>
        new UploadProductImage(new TenantAccessService(_repo, _user, _platform), _products)
            .ExecuteAsync("elika", fileName, contentType, length, Stream.Null, CancellationToken.None);

    [Fact]
    public async Task Member_upload_returns_shopify_url_with_safe_name()
    {
        var result = await Upload("Tiara Rosa (1).JPG", "IMAGE/JPEG", 1000);

        var staged = Assert.Single(_products.StagedImages);
        Assert.Equal(("elika", "tiara-rosa-1.jpg", "image/jpeg"), staged);
        Assert.EndsWith("tiara-rosa-1.jpg", result.Url);
    }

    [Theory]
    [InlineData("doc.pdf", "application/pdf", 1000)]
    [InlineData("foto.jpg", null, 1000)]
    [InlineData("foto.jpg", "image/jpeg", 0)]
    [InlineData("foto.jpg", "image/jpeg", ImageFiles.MaxBytes + 1)]
    public async Task Invalid_files_are_rejected_before_shopify(string fileName, string? contentType, long length)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => Upload(fileName, contentType, length));

        Assert.Equal(AppErrorKind.Validation, ex.Kind);
        Assert.Empty(_products.StagedImages);
    }

    [Fact]
    public async Task Non_member_cannot_upload()
    {
        _user.Uid = "outra-pessoa";

        var ex = await Assert.ThrowsAsync<AppException>(() => Upload("foto.jpg", "image/jpeg", 1000));

        Assert.Equal(AppErrorKind.Forbidden, ex.Kind);
        Assert.Empty(_products.StagedImages);
    }

    [Theory]
    [InlineData("Laço Pérola.png", "image/png", "laco-perola.png")]
    [InlineData("???.webp", "image/webp", "foto.webp")]
    [InlineData(null, "image/heic", "foto.heic")]
    [InlineData("  já--foi  ", "image/jpeg", "ja-foi.jpg")]
    public void Safe_name_keeps_only_letters_and_numbers(string? fileName, string contentType, string expected) =>
        Assert.Equal(expected, ImageFiles.SafeName(fileName, contentType));
}
