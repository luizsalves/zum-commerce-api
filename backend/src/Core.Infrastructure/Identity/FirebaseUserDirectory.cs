using Core.Application.Interfaces;
using FirebaseAdmin.Auth;

namespace Core.Infrastructure.Identity;

public sealed class FirebaseUserDirectory : IUserDirectory
{
    public async Task<DirectoryUser?> FindByEmailAsync(string email, CancellationToken ct)
    {
        try
        {
            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, ct);
            return new DirectoryUser(user.Uid, user.Email);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            return null;
        }
    }
}
