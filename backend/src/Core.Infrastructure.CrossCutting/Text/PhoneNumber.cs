using System.Text.RegularExpressions;

namespace Core.Infrastructure.CrossCutting.Text;

public static partial class PhoneNumber
{
    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();

    /// <summary>
    /// Converte telefone brasileiro para E.164 (formato exigido pela Shopify).
    /// "(35) 99999-9999" → "+5535999999999". Devolve null se vazio; false em <paramref name="valid"/> se inválido.
    /// </summary>
    public static string? ToE164Brazil(string? input, out bool valid)
    {
        valid = true;
        if (string.IsNullOrWhiteSpace(input)) return null;

        var digits = NonDigits().Replace(input, "");
        if (digits.StartsWith("55") && digits.Length is 12 or 13) return "+" + digits;
        if (digits.Length is 10 or 11) return "+55" + digits;

        valid = false;
        return null;
    }
}
