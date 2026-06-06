using CoreForge.Domain.Common;

namespace CoreForge.Domain.Entities;

public enum TenantPlan { Free = 0, Starter = 1, Professional = 2, Enterprise = 3 }

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantPlan Plan { get; set; } = TenantPlan.Free;
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
}
