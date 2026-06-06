using CoreForge.Domain.Entities;

namespace CoreForge.Application.Payments.DTOs;

public record CreateCheckoutSessionRequest(
    TenantPlan Plan,
    string SuccessUrl,
    string CancelUrl
);
