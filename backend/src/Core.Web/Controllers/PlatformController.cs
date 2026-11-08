using Core.Application.Contracts.Tenants;
using Core.Application.UseCases.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers;

/// <summary>Área da Zum: cadastrar lojas de clientes e liberar acessos.</summary>
[ApiController]
[Authorize]
[Route("api/platform/tenants")]
public sealed class PlatformController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request, [FromServices] CreateTenant useCase, CancellationToken ct)
    {
        var id = await useCase.ExecuteAsync(request, ct);
        return Created($"/api/platform/tenants/{id}", new { id });
    }

    [HttpPut("{tenantId}/credentials")]
    public async Task<IActionResult> UpdateCredentials(
        string tenantId, [FromBody] UpdateCredentialsRequest request, [FromServices] UpdateTenantCredentials useCase, CancellationToken ct)
    {
        await useCase.ExecuteAsync(tenantId, request, ct);
        return NoContent();
    }

    [HttpPost("{tenantId}/test")]
    public Task<ShopInfoResponse> Test(string tenantId, [FromServices] TestTenantConnection useCase, CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, ct);

    [HttpPost("{tenantId}/members")]
    public Task<MemberResponse> AddMember(
        string tenantId, [FromBody] AddMemberRequest request, [FromServices] AddTenantMember useCase, CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, request, ct);
}
