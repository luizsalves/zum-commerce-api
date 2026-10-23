namespace Core.Domain.Constants;

public static class CredentialModes
{
    /// <summary>App criado no Dev Dashboard da organização da loja (client_id + client_secret → token de 24h).</summary>
    public const string ClientCredentials = "client_credentials";

    /// <summary>Token já emitido (ex.: app OAuth da Zum).</summary>
    public const string AccessToken = "access_token";
}
