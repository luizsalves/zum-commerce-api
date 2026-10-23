using Core.Application.Configuration;
using Core.Application.Exceptions;
using Core.Application.Services;
using Core.Domain.Constants;
using Core.Domain.Tenants;
using Core.Web.Tests.Fakes;

namespace Core.Web.Tests.Tenants;

public class TenantAccessServiceTests
{
    private readonly InMemoryTenantRepository _repo = new();
    private readonly FakeCurrentUser _user = new();
    private readonly PlatformOptions _platform = new() { AdminEmails = "dono@zum.com.br" };

    private TenantAccessService Service() => new(_repo, _user, _platform);

    [Fact]
    public async Task Member_can_access_own_store()
    {
        _repo.Seed("elika");
        _repo.Members.Add(new TenantMember("elika", "user-1", "lojista@exemplo.com", MemberRoles.Owner));

        var tenant = await Service().RequireMemberAsync("elika", CancellationToken.None);
        Assert.Equal("elika", tenant.Id);
    }

    [Fact]
    public async Task Non_member_is_forbidden()
    {
        _repo.Seed("elika");
        var ex = await Assert.ThrowsAsync<AppException>(() => Service().RequireMemberAsync("elika", CancellationToken.None));
        Assert.Equal(AppErrorKind.Forbidden, ex.Kind);
    }

    [Fact]
    public async Task Platform_admin_accesses_any_store()
    {
        _repo.Seed("elika");
        _user.Email = "dono@zum.com.br";
        await Service().RequireMemberAsync("elika", CancellationToken.None);
    }

    [Fact]
    public void Platform_admin_requires_verified_email()
    {
        _user.Email = "dono@zum.com.br";
        _user.EmailVerified = false;
        Assert.False(Service().IsPlatformAdmin);
    }

    [Fact]
    public async Task Inactive_store_is_not_found()
    {
        _repo.Seed("elika", active: false);
        var ex = await Assert.ThrowsAsync<AppException>(() => Service().RequireTenantAsync("elika", CancellationToken.None));
        Assert.Equal(AppErrorKind.NotFound, ex.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Elika Loja")]
    [InlineData("../x")]
    public async Task Invalid_tenant_id_is_rejected(string? id)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => Service().RequireTenantAsync(id, CancellationToken.None));
        Assert.Equal(AppErrorKind.Validation, ex.Kind);
    }

    [Fact]
    public void Anonymous_user_is_unauthorized()
    {
        _user.IsAuthenticated = false;
        var ex = Assert.Throws<AppException>(() => Service().RequireUid());
        Assert.Equal(AppErrorKind.Unauthorized, ex.Kind);
    }
}
