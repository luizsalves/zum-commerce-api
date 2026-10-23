using Core.Application.Common;
using Core.Application.Contracts.Customers;
using Core.Application.UseCases.Customers;
using Core.Domain.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers;

/// <summary>Vitrine: cadastro obrigatório do cliente final para poder comprar.</summary>
[ApiController]
[Authorize]
[Route("api/store/customers")]
public sealed class StoreCustomersController : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<StoreCustomer>> Me(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromServices] GetMyCustomerProfile useCase,
        CancellationToken ct)
    {
        var customer = await useCase.ExecuteAsync(tenantId, ct);
        if (customer is null) return NotFound(new { error = "Cadastro não encontrado." });
        return Ok(customer);
    }

    [HttpPost("register")]
    public Task<StoreCustomer> Register(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromBody] RegisterCustomerRequest request,
        [FromServices] RegisterCustomer useCase,
        CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, request, ct);
}
