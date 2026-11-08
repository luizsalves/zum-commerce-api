using System.Text;
using System.Text.Json;
using Core.Infrastructure.Shopify;

namespace Core.Web.Tests.Shopify;

public class StagedUploadFormTests
{
    private static readonly JsonElement Parameters = JsonDocument.Parse("""
        [{ "name": "key", "value": "tmp/1/tiara.jpg" }, { "name": "policy", "value": "abc==" }]
        """).RootElement;

    private static async Task<(string ContentType, string Body)> Build()
    {
        using var form = ShopifyProductGateway.BuildUploadForm(
            Parameters, new MemoryStream([1, 2, 3]), "image/jpeg", "tiara.jpg");
        return (form.Headers.ContentType!.ToString(), Encoding.UTF8.GetString(await form.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task Boundary_is_not_quoted()
    {
        var (contentType, body) = await Build();

        Assert.DoesNotContain("\"", contentType);
        var boundary = contentType.Split("boundary=")[1];
        Assert.StartsWith("--" + boundary, body);
    }

    [Fact]
    public async Task Fields_come_before_the_file_without_extra_headers()
    {
        var (_, body) = await Build();

        Assert.Contains("Content-Disposition: form-data; name=\"key\"\r\n\r\ntmp/1/tiara.jpg", body);
        Assert.Contains("Content-Disposition: form-data; name=\"file\"; filename=\"tiara.jpg\"", body);
        Assert.DoesNotContain("filename*", body);
        Assert.True(body.IndexOf("name=\"policy\"", StringComparison.Ordinal) < body.IndexOf("name=\"file\"", StringComparison.Ordinal));
    }
}
