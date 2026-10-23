namespace Core.Application.Configuration;

/// <summary>Configuração da plataforma (seção "Platform").</summary>
public sealed class PlatformOptions
{
    public const string SectionName = "Platform";

    /// <summary>E-mails da equipe da Zum, separados por vírgula. Eles cadastram lojas e liberam acessos.</summary>
    public string AdminEmails { get; set; } = "";

    public IReadOnlySet<string> AdminEmailSet => AdminEmails
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(e => e.ToLowerInvariant())
        .ToHashSet();
}
