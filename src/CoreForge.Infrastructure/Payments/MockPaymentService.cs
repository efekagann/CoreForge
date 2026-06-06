using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Entities;

namespace CoreForge.Infrastructure.Payments;

public class MockPaymentService : IPaymentService
{
    public string ProviderName => "Mock";

    public Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid tenantId, TenantPlan plan, string successUrl, string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var result = new CheckoutSessionResult(
            IsSuccess: true,
            SessionId: $"mock_{Guid.NewGuid():N}",
            CheckoutUrl: null,
            RequiresRedirect: false,
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<WebhookEventResult> ProcessWebhookAsync(
        string payload, string signature,
        CancellationToken cancellationToken = default)
    {
        // Mock provider has no real webhooks; return a no-op success.
        var result = new WebhookEventResult(
            IsSuccess: true,
            EventType: "mock.noop",
            TenantId: null,
            NewPlan: null,
            Amount: null,
            Currency: "USD",
            ExternalId: string.Empty);

        return Task.FromResult(result);
    }
}
