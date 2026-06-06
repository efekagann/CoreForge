using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Entities;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace CoreForge.Infrastructure.Payments;

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public string ProviderName => "Stripe";

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid tenantId, TenantPlan plan, string successUrl, string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString(),
                    ["plan"]      = plan.ToString()
                },
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price    = GetPriceId(plan),
                        Quantity = 1
                    }
                ]
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new CheckoutSessionResult(true, session.Id, session.Url, true, null);
        }
        catch (StripeException ex)
        {
            return new CheckoutSessionResult(false, string.Empty, null, false, ex.Message);
        }
    }

    public Task<WebhookEventResult> ProcessWebhookAsync(
        string payload, string signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _settings.WebhookSecret);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = (Session)stripeEvent.Data.Object;
                session.Metadata.TryGetValue("tenant_id", out var tenantIdStr);
                session.Metadata.TryGetValue("plan", out var planStr);

                Guid.TryParse(tenantIdStr, out var tenantId);
                Enum.TryParse<TenantPlan>(planStr, out var plan);

                var amountTotal = session.AmountTotal.HasValue
                    ? (decimal)session.AmountTotal.Value / 100m
                    : (decimal?)null;

                var result = new WebhookEventResult(
                    IsSuccess: true,
                    EventType: stripeEvent.Type,
                    TenantId: tenantId == Guid.Empty ? null : tenantId,
                    NewPlan: plan,
                    Amount: amountTotal,
                    Currency: (session.Currency ?? "usd").ToUpperInvariant(),
                    ExternalId: session.PaymentIntentId ?? session.Id);

                return Task.FromResult(result);
            }

            // Unhandled event type — acknowledge without action
            return Task.FromResult(new WebhookEventResult(true, stripeEvent.Type, null, null, null, "USD", string.Empty));
        }
        catch (StripeException ex)
        {
            return Task.FromResult(new WebhookEventResult(false, "error", null, null, null, "USD", ex.Message));
        }
    }

    // Developer replaces these Price IDs with actual Stripe Price IDs from the dashboard.
    private static string GetPriceId(TenantPlan plan) => plan switch
    {
        TenantPlan.Starter      => "price_starter_monthly",
        TenantPlan.Professional => "price_professional_monthly",
        TenantPlan.Enterprise   => "price_enterprise_monthly",
        _ => throw new ArgumentException($"No Stripe Price ID configured for plan: {plan}")
    };
}
