using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;

namespace CoreForge.Application.Payments.DTOs;

public record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    TenantPlan Plan,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime? EndDate,
    string? ExternalSubscriptionId
);
