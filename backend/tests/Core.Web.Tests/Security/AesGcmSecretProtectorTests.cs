using System.Security.Cryptography;
using Core.Infrastructure.Security;

namespace Core.Web.Tests.Security;

public class AesGcmSecretProtectorTests
{
    private static AesGcmSecretProtector Create() =>
        new(new SecretsOptions { Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) });

    [Fact]
    public void Protect_and_unprotect_returns_original_value()
    {
        var protector = Create();
        var secret = "shpss_segredo-da-loja";

        var stored = protector.Protect(secret);

        Assert.NotEqual(secret, stored);
        Assert.Equal(secret, protector.Unprotect(stored));
    }

    [Fact]
    public void Protect_uses_random_nonce()
    {
        var protector = Create();
        Assert.NotEqual(protector.Protect("abc"), protector.Protect("abc"));
    }

    [Fact]
    public void Unprotect_with_other_key_fails()
    {
        var stored = Create().Protect("abc");
        Assert.ThrowsAny<CryptographicException>(() => Create().Unprotect(stored));
    }

    [Fact]
    public void Rejects_key_with_wrong_size()
    {
        var options = new SecretsOptions { Key = Convert.ToBase64String(new byte[16]) };
        Assert.Throws<InvalidOperationException>(() => new AesGcmSecretProtector(options));
    }
}
