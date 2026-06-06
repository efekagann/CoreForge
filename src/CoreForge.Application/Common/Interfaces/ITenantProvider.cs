namespace CoreForge.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid? CurrentTenantId { get; }
    void SetTenant(Guid tenantId);
}
