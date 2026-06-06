using CoreForge.Domain.Entities;

namespace CoreForge.Application.Common.Interfaces;

public interface IFeatureService
{
    bool IsFeatureEnabled(TenantPlan plan, string featureName);
    int GetLimit(TenantPlan plan, string limitName);
}
