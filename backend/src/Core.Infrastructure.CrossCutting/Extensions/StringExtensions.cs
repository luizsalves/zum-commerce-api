namespace Core.Infrastructure.CrossCutting.Extensions;

public static class StringExtensions
{
    public static string? TrimToNull(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeEmail(this string value) => value.Trim().ToLowerInvariant();
}
