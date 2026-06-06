using AutoMapper;
using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using CoreForge.Application.Payments.DTOs;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Payments.Queries.GetCurrentSubscription;

public class GetCurrentSubscriptionQueryHandler(
    IRepository<Subscription> repository,
    IMapper mapper
) : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        // Global query filter scopes this to current tenant automatically.
        var subscriptions = await repository.FindAsync(
            s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing,
            cancellationToken);

        var current = subscriptions.FirstOrDefault()
            ?? throw new NotFoundException(ResourceKeys.Payment.SubscriptionNotFound);

        return mapper.Map<SubscriptionDto>(current);
    }
}
