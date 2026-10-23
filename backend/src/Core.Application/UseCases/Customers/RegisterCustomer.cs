using Core.Application.Contracts.Customers;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain.Customers;
using Core.Infrastructure.CrossCutting.Extensions;
using Core.Infrastructure.CrossCutting.Text;

namespace Core.Application.UseCases.Customers;

/// <summary>
/// Completa o cadastro do cliente na vitrine: cria (ou reaproveita, se o e-mail já existir)
/// o Customer na Shopify e grava o vínculo com a conta de login.
/// </summary>
public sealed class RegisterCustomer(
    TenantAccessService access,
    ICurrentUser user,
    IShopifyCustomerGateway shopifyCustomers,
    ICustomerProfileRepository profiles)
{
    public async Task<StoreCustomer> ExecuteAsync(string? tenantId, RegisterCustomerRequest request, CancellationToken ct)
    {
        var uid = access.RequireUid();
        var tenant = await access.RequireTenantAsync(tenantId, ct);

        var email = user.Email?.NormalizeEmail()
            ?? throw AppException.Validation("Sua conta não tem e-mail. Entre com Google, Facebook ou e-mail e senha.");
        if (string.IsNullOrWhiteSpace(request.FirstName)) throw AppException.Validation("Informe seu nome.");

        var phone = PhoneNumber.ToE164Brazil(request.Phone, out var phoneValid);
        if (!phoneValid) throw AppException.Validation("Telefone inválido. Use DDD + número.");

        var shopifyId = await shopifyCustomers.FindIdByEmailAsync(tenant.Id, email, ct)
            ?? await shopifyCustomers.CreateAsync(tenant.Id, new NewShopifyCustomer(
                request.FirstName.Trim(),
                request.LastName.TrimToNull(),
                email,
                phone,
                request.AcceptsMarketing), ct);

        var customer = new StoreCustomer
        {
            ShopifyCustomerId = shopifyId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName?.Trim() ?? "",
            Email = email,
            Phone = phone,
            AcceptsMarketing = request.AcceptsMarketing,
        };
        await profiles.SaveAsync(tenant.Id, uid, customer, ct);
        return customer;
    }
}
