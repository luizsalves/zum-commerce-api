using System.Globalization;
using System.Text.Json;

namespace Core.Infrastructure.Shopify;

/// <summary>Leitura tolerante de respostas GraphQL (campos podem vir nulos ou ausentes).</summary>
internal static class ShopifyJson
{
    public static JsonElement? Walk(this JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var key in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(key, out var next)) return null;
            current = next;
        }
        return current.ValueKind == JsonValueKind.Null ? null : current;
    }

    public static string? Str(this JsonElement element, params string[] path) =>
        element.Walk(path) is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;

    public static decimal? Dec(this JsonElement element, params string[] path)
    {
        if (element.Walk(path) is not { } v) return null;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) => d,
            _ => null,
        };
    }

    public static int? Int(this JsonElement element, params string[] path) =>
        element.Walk(path) is { ValueKind: JsonValueKind.Number } v ? v.GetInt32() : null;

    public static long Long(this JsonElement element, params string[] path)
    {
        if (element.Walk(path) is not { } v) return 0;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetInt64(),
            JsonValueKind.String when long.TryParse(v.GetString(), out var n) => n,
            _ => 0,
        };
    }

    public static bool Bool(this JsonElement element, params string[] path) =>
        element.Walk(path) is { ValueKind: JsonValueKind.True };
}
