using CoreForge.Domain.Entities;

namespace CoreForge.Application.Common.Interfaces;

public record CheckoutSessionResult(
    bool IsSuccess,
    string SessionId,
    string? CheckoutUrl,
    bool RequiresRedirect,
    string? ErrorMessage);

public record WebhookEventResult(
    bool IsSuccess,
    string EventType,
    Guid? TenantId,
    TenantPlan? NewPlan,
    decimal? Amount,
    string Currency,
    string ExternalId);

public interface IPaymentService
{
    string ProviderName { get; }

    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid tenantId,
        TenantPlan plan,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<WebhookEventResult> ProcessWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);
}
