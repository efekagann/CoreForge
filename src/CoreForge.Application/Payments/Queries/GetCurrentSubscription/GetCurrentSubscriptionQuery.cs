using CoreForge.Application.Payments.DTOs;
using MediatR;

namespace CoreForge.Application.Payments.Queries.GetCurrentSubscription;

public record GetCurrentSubscriptionQuery : IRequest<SubscriptionDto>;
