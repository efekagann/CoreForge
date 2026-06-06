using CoreForge.Application.Payments.DTOs;
using CoreForge.Domain.Entities;
using MediatR;

namespace CoreForge.Application.Payments.Commands.CreateCheckoutSession;

public record CreateCheckoutSessionCommand(
    Guid TenantId,
    TenantPlan Plan,
    string SuccessUrl,
    string CancelUrl
) : IRequest<CheckoutSessionDto>;
