namespace Core.Application.Interfaces;

/// <summary>Usuário autenticado na requisição atual (token do Firebase).</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? Uid { get; }
    string? Email { get; }
    bool EmailVerified { get; }
    string? DisplayName { get; }
}
