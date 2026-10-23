using Core.Application.Common;
using Core.Application.Contracts.Products;
using Core.Application.UseCases.Products;
using Core.Domain.Catalog;
using Core.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers;

/// <summary>Painel do lojista: produtos. Toda rota exige login + X-Tenant-Id de uma loja que o usuário administra.</summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public sealed class AdminProductsController : ControllerBase
{
    [HttpGet("products")]
    public Task<PagedResult<ProductSummary>> List(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromQuery] string? q,
        [FromQuery] string? after,
        [FromServices] ListProducts useCase,
        CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, q, after, ct);

    // O id da Shopify (gid://shopify/Product/123) vai na query string para não brigar com as barras da rota.
    [HttpGet("product")]
    public Task<Product> Get(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromQuery] string id,
        [FromServices] GetProduct useCase,
        CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, id, ct);

    [HttpPost("product")]
    public Task<Product> Save(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromBody] Product product,
        [FromServices] SaveProduct useCase,
        CancellationToken ct) =>
        useCase.ExecuteAsync(tenantId, product, ct);

    [HttpPost("product/status")]
    public async Task<IActionResult> SetStatus(
        [FromHeader(Name = TenantIds.HeaderName)] string? tenantId,
        [FromQuery] string id,
        [FromBody] SetProductStatusRequest request,
        [FromServices] SetProductStatus useCase,
        CancellationToken ct)
    {
        await useCase.ExecuteAsync(tenantId, id, request.Status, ct);
        return NoContent();
    }
}
