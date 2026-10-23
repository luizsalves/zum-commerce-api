namespace Core.Application.Contracts.Tenants;

public sealed record CreateTenantRequest(
    string Id,
    string Name,
    string ShopDomain,
    string StorefrontToken,
    string ClientId,
    string ClientSecret,
    List<string>? Domains,
    string? PrimaryColor,
    string? Whatsapp);

public sealed record UpdateCredentialsRequest(string? ClientId, string? ClientSecret, string? AccessToken);

public sealed record AddMemberRequest(string Email, string? Role);

public sealed record MemberResponse(string Uid, string? Email, string Role);

public sealed record TenantSummaryResponse(string Id, string Name, string ShopDomain, bool Active);

public sealed record MeResponse(bool PlatformAdmin, IReadOnlyList<TenantSummaryResponse> Tenants);

public sealed record ShopInfoResponse(string Name, string MyshopifyDomain, string? CurrencyCode, string? PlanName);
