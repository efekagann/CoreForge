using CoreForge.Domain.Common;

namespace CoreForge.Domain.Entities;

public class PaymentTransaction : AuditableEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ExternalTransactionId { get; set; }
    public string? Description { get; set; }
    public TenantPlan Plan { get; set; }
}
