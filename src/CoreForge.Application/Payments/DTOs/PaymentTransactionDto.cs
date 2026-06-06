using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;

namespace CoreForge.Application.Payments.DTOs;

public record PaymentTransactionDto(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    TenantPlan Plan,
    string? Description,
    string? ExternalTransactionId,
    DateTime CreatedAt
);
