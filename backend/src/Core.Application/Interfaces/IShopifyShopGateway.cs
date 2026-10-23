using Core.Application.Contracts.Tenants;

namespace Core.Application.Interfaces;

public interface IShopifyShopGateway
{
    /// <summary>Consulta dados básicos da loja. Serve para testar se as credenciais funcionam.</summary>
    Task<ShopInfoResponse> GetShopInfoAsync(string tenantId, CancellationToken ct);

    /// <summary>Descarta o token em cache (ex.: depois de trocar as credenciais).</summary>
    void InvalidateToken(string tenantId);
}
