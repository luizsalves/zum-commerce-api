using Core.Application.Interfaces;

namespace Core.Web.Authentication;

/// <summary>Lê o usuário a partir das claims do token do Firebase.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? Uid => Principal?.FindFirst("user_id")?.Value ?? Principal?.FindFirst("sub")?.Value;

    public string? Email => Principal?.FindFirst("email")?.Value?.Trim().ToLowerInvariant();

    public bool EmailVerified =>
        string.Equals(Principal?.FindFirst("email_verified")?.Value, "true", StringComparison.OrdinalIgnoreCase);

    public string? DisplayName => Principal?.FindFirst("name")?.Value;
}
