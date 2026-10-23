using Core.Application.Exceptions;

namespace Core.Application.Common;

public static class ShopDomains
{
    /// <summary>"https://Elika.myshopify.com/" ou "elika" → "elika.myshopify.com".</summary>
    public static string Normalize(string input)
    {
        var shop = (input ?? "").Trim().ToLowerInvariant()
            .Replace("https://", "")
            .Replace("http://", "")
            .TrimEnd('/');

        if (shop.Length == 0) throw AppException.Validation("Informe o domínio .myshopify.com da loja.");
        if (!shop.Contains('.')) shop += ".myshopify.com";
        if (!shop.EndsWith(".myshopify.com", StringComparison.Ordinal))
            throw AppException.Validation("Use o domínio .myshopify.com da loja (ex.: elika.myshopify.com).");
        return shop;
    }
}
