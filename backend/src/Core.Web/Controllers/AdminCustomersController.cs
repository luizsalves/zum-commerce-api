using Core.Application.Common;
using Core.Application.UseCases.Customers;
using Core.Domain.Common;
using Core.Domain.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers;

/// <summary>Painel do lojista: clientes da loja (dados da Shopify).</summary>
[ApiController]
[Authorize]
[Route("api/admin/customers")]
public sealed class AdminCustomersController : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CustomerSummary>> List(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromQuery] string? q,
        [FromQuery] string? after,
        [FromServices] ListCustomers useCase,
        CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, q, after, ct);
}
