using Core.Application.Interfaces;

namespace Core.Web.Tests.Fakes;

public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public string? Uid { get; set; } = "user-1";
    public string? Email { get; set; } = "lojista@exemplo.com";
    public bool EmailVerified { get; set; } = true;
    public string? DisplayName { get; set; } = "Lojista";
}
