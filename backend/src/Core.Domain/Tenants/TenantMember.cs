namespace Core.Domain.Tenants;

/// <summary>Pessoa com acesso ao painel de uma loja.</summary>
public sealed record TenantMember(string TenantId, string Uid, string Email, string Role);
