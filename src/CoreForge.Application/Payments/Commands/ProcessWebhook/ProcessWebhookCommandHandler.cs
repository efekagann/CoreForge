using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Common.Localization;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Payments.Commands.ProcessWebhook;

public class ProcessWebhookCommandHandler(
    IPaymentService paymentService,
    IRepository<Subscription> subscriptionRepo,
    IRepository<PaymentTransaction> transactionRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<ProcessWebhookCommand>
{
    public async Task Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        var result = await paymentService.ProcessWebhookAsync(request.Payload, request.Signature, cancellationToken);

        if (!result.IsSuccess)
            throw new ConflictException(ResourceKeys.Payment.WebhookInvalid);

        // Only act on events that carry TenantId and plan info
        if (result.TenantId is null || result.NewPlan is null)
            return;

        var tenantId = result.TenantId.Value;
        var newPlan = result.NewPlan.Value;

        var transaction = new PaymentTransaction
        {
            TenantId = tenantId,
            Amount = result.Amount ?? 0,
            Currency = result.Currency,
            Status = PaymentStatus.Succeeded,
            ExternalTransactionId = result.ExternalId,
            Description = $"Webhook: {result.EventType}",
            Plan = newPlan
        };

        await transactionRepo.AddAsync(transaction, cancellationToken);

        // Upsert subscription
        var existing = await subscriptionRepo.FindAsync(
            s => s.TenantId == tenantId, cancellationToken);

        var subscription = existing.FirstOrDefault() ?? new Subscription { TenantId = tenantId };
        subscription.Plan = newPlan;
        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDate = DateTime.UtcNow;
        subscription.ExternalSubscriptionId = result.ExternalId;

        if (existing.Count == 0)
            await subscriptionRepo.AddAsync(subscription, cancellationToken);
        else
            subscriptionRepo.Update(subscription);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
