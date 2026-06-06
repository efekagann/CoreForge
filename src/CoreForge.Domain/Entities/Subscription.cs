using CoreForge.Domain.Common;

namespace CoreForge.Domain.Entities;

public class Subscription : AuditableEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public TenantPlan Plan { get; set; } = TenantPlan.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string? ExternalSubscriptionId { get; set; }
}
