using Core.Application.Configuration;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Application.UseCases.Customers;
using Core.Application.UseCases.Products;
using Core.Application.UseCases.Tenants;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence.Firestore.Repositories;
using Core.Infrastructure.Security;
using Core.Infrastructure.Shopify;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.IoC;

public static class DependencyInjection
{
    /// <summary>Registra Firebase, Firestore, Shopify, segurança, serviços e casos de uso.</summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
            throw new InvalidOperationException("Configure Firebase:ProjectId (env Firebase__ProjectId).");

        // Configurações
        services.AddSingleton(configuration.GetSection(PlatformOptions.SectionName).Get<PlatformOptions>() ?? new PlatformOptions());
        services.AddSingleton(configuration.GetSection(SecretsOptions.SectionName).Get<SecretsOptions>() ?? new SecretsOptions());

        // Firebase / Firestore. No Cloud Run as credenciais vêm da service account; local, de
        // GOOGLE_APPLICATION_CREDENTIALS ou "gcloud auth application-default login".
        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = projectId,
            });
        }
        services.AddSingleton(_ => FirestoreDb.Create(projectId));

        // Infraestrutura
        services.AddMemoryCache();
        services.AddHttpClient(ShopifyHttp.ClientName, c => c.Timeout = TimeSpan.FromSeconds(60));
        services.AddSingleton<ISecretProtector, AesGcmSecretProtector>();
        services.AddSingleton<IUserDirectory, FirebaseUserDirectory>();
        services.AddSingleton<ITenantRepository, FirestoreTenantRepository>();
        services.AddSingleton<ICustomerProfileRepository, FirestoreCustomerProfileRepository>();
        services.AddSingleton<ShopifyTokenProvider>();
        services.AddSingleton<ShopifyGraphQLClient>();
        services.AddSingleton<IShopifyProductGateway, ShopifyProductGateway>();
        services.AddSingleton<IShopifyCustomerGateway, ShopifyCustomerGateway>();
        services.AddSingleton<IShopifyShopGateway, ShopifyShopGateway>();

        // Aplicação (por requisição, porque dependem do usuário logado)
        services.AddScoped<TenantAccessService>();

        services.AddScoped<GetMe>();
        services.AddScoped<CreateTenant>();
        services.AddScoped<UpdateTenantCredentials>();
        services.AddScoped<AddTenantMember>();
        services.AddScoped<TestTenantConnection>();

        services.AddScoped<ListProducts>();
        services.AddScoped<GetProduct>();
        services.AddScoped<SaveProduct>();
        services.AddScoped<SetProductStatus>();

        services.AddScoped<ListCustomers>();
        services.AddScoped<GetMyCustomerProfile>();
        services.AddScoped<RegisterCustomer>();

        return services;
    }
}
