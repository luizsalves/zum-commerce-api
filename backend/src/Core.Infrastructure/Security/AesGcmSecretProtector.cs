using System.Security.Cryptography;
using System.Text;
using Core.Application.Interfaces;

namespace Core.Infrastructure.Security;

/// <summary>AES-256-GCM. Formato gravado: base64(nonce[12] | tag[16] | cifra).</summary>
public sealed class AesGcmSecretProtector : ISecretProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesGcmSecretProtector(SecretsOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Key))
            throw new InvalidOperationException("Configure Secrets:Key (env Secrets__Key) com 32 bytes em base64.");
        _key = Convert.FromBase64String(options.Key);
        if (_key.Length != 32)
            throw new InvalidOperationException("Secrets:Key precisa ter exatamente 32 bytes (base64).");
    }

    public string Protect(string plainText)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plain = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using (var aes = new AesGcm(_key, TagSize))
            aes.Encrypt(nonce, plain, cipher, tag);

        var output = new byte[NonceSize + TagSize + cipher.Length];
        nonce.CopyTo(output, 0);
        tag.CopyTo(output, NonceSize);
        cipher.CopyTo(output, NonceSize + TagSize);
        return Convert.ToBase64String(output);
    }

    public string Unprotect(string protectedText)
    {
        var data = Convert.FromBase64String(protectedText);
        if (data.Length < NonceSize + TagSize) throw new CryptographicException("Segredo em formato inválido.");

        var plain = new byte[data.Length - NonceSize - TagSize];
        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Decrypt(
                data.AsSpan(0, NonceSize),
                data.AsSpan(NonceSize + TagSize),
                data.AsSpan(NonceSize, TagSize),
                plain);
        }
        return Encoding.UTF8.GetString(plain);
    }
}
