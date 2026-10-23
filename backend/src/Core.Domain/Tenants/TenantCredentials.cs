namespace Core.Domain.Tenants;

/// <summary>Credenciais da Admin API da loja. Os segredos ficam sempre criptografados.</summary>
public sealed class TenantCredentials
{
    public required string Mode { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecretEncrypted { get; init; }
    public string? AccessTokenEncrypted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
