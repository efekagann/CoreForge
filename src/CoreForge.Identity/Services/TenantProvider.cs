using CoreForge.Application.Common.Interfaces;

namespace CoreForge.Identity.Services;

public class TenantProvider : ITenantProvider
{
    public Guid? CurrentTenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        CurrentTenantId = tenantId;
    }
}
