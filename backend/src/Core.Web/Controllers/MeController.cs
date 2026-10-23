using Core.Application.Contracts.Tenants;
using Core.Application.UseCases.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    /// <summary>Quem sou eu no painel e quais lojas administro.</summary>
    [HttpGet]
    public Task<MeResponse> Get([FromServices] GetMe useCase, CancellationToken ct) => useCase.ExecuteAsync(ct);
}
