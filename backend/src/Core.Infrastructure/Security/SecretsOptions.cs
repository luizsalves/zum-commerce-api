namespace Core.Infrastructure.Security;

/// <summary>Seção "Secrets". Key = 32 bytes em base64 (gere com: openssl rand -base64 32).</summary>
public sealed class SecretsOptions
{
    public const string SectionName = "Secrets";
    public string Key { get; set; } = "";
}
