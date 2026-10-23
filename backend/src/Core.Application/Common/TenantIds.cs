using System.Text.RegularExpressions;

namespace Core.Application.Common;

public static partial class TenantIds
{
    /// <summary>Cabeçalho HTTP que identifica a loja em cada requisição.</summary>
    public const string HeaderName = "X-Tenant-Id";

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{1,40}$")]
    private static partial Regex Pattern();

    public static bool IsValid(string? id) => id is not null && Pattern().IsMatch(id);
}
