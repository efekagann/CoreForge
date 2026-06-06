using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Interfaces;
using CoreForge.Application.Common.Localization;
using CoreForge.Application.Payments.DTOs;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Payments.Commands.CreateCheckoutSession;

public class CreateCheckoutSessionCommandHandler(
    IPaymentService paymentService,
    IRepository<Subscription> subscriptionRepo,
    IRepository<PaymentTransaction> transactionRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>
{
    public async Task<CheckoutSessionDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var result = await paymentService.CreateCheckoutSessionAsync(
            request.TenantId, request.Plan, request.SuccessUrl, request.CancelUrl, cancellationToken);

        if (!result.IsSuccess)
            throw new ConflictException(ResourceKeys.Payment.CheckoutFailed);

        var transaction = new PaymentTransaction
        {
            TenantId = request.TenantId,
            Amount = 0,
            Currency = "USD",
            Status = result.RequiresRedirect ? PaymentStatus.Pending : PaymentStatus.Succeeded,
            ExternalTransactionId = result.SessionId,
            Description = $"{paymentService.ProviderName} checkout — {request.Plan}",
            Plan = request.Plan
        };

        await transactionRepo.AddAsync(transaction, cancellationToken);

        if (!result.RequiresRedirect)
        {
            // Mock / immediate provider: activate subscription right away
            var existing = await subscriptionRepo.FindAsync(
                s => s.TenantId == request.TenantId, cancellationToken);

            var subscription = existing.FirstOrDefault() ?? new Subscription { TenantId = request.TenantId };
            subscription.Plan = request.Plan;
            subscription.Status = SubscriptionStatus.Active;
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = null;
            subscription.ExternalSubscriptionId = result.SessionId;

            if (existing.Count == 0)
                await subscriptionRepo.AddAsync(subscription, cancellationToken);
            else
                subscriptionRepo.Update(subscription);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckoutSessionDto(result.SessionId, result.CheckoutUrl, result.RequiresRedirect, paymentService.ProviderName);
    }
}
