using System.Globalization;

namespace Core.Infrastructure.CrossCutting.Extensions;

public static class DecimalExtensions
{
    /// <summary>A Admin API da Shopify espera valores monetários como string ("79.90").</summary>
    public static string ToShopifyMoney(this decimal value) =>
        Math.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);
}
