namespace Core.Application.Interfaces;

/// <summary>Criptografa segredos das lojas antes de gravar no banco.</summary>
public interface ISecretProtector
{
    string Protect(string plainText);
    string Unprotect(string protectedText);
}
