using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Core.Web.Authentication;

public static class FirebaseAuthenticationExtensions
{
    /// <summary>Valida o ID token do Firebase Auth enviado pelo front (Authorization: Bearer ...).</summary>
    public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"]
            ?? throw new InvalidOperationException("Configure Firebase:ProjectId.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://securetoken.google.com/{projectId}";
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{projectId}",
                    ValidateAudience = true,
                    ValidAudience = projectId,
                    ValidateLifetime = true,
                };
            });

        services.AddAuthorization();
        return services;
    }
}
