namespace Core.Application.Interfaces;

public sealed record DirectoryUser(string Uid, string? Email);

/// <summary>Contas de login existentes (Firebase Auth).</summary>
public interface IUserDirectory
{
    Task<DirectoryUser?> FindByEmailAsync(string email, CancellationToken ct);
}
