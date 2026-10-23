namespace Core.Domain.Constants;

public static class MemberRoles
{
    /// <summary>Dono da loja.</summary>
    public const string Owner = "owner";

    /// <summary>Funcionário com acesso ao painel.</summary>
    public const string Admin = "admin";

    public static string Normalize(string? role) => role == Owner ? Owner : Admin;
}
