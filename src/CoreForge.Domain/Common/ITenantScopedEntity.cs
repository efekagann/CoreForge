namespace CoreForge.Domain.Common;

public interface ITenantScopedEntity
{
    Guid TenantId { get; set; }
}
