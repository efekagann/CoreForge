using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Common;
using CoreForge.Domain.Entities;

namespace CoreForge.Infrastructure.Payments;

public class FeatureService : IFeatureService
{
    private static readonly IReadOnlyDictionary<TenantPlan, IReadOnlySet<string>> PlanFeatures =
        new Dictionary<TenantPlan, IReadOnlySet<string>>
        {
            [TenantPlan.Free] = new HashSet<string>
            {
                Features.BasicApi
            },
            [TenantPlan.Starter] = new HashSet<string>
            {
                Features.BasicApi, Features.Analytics, Features.TeamMembers
            },
            [TenantPlan.Professional] = new HashSet<string>
            {
                Features.BasicApi, Features.Analytics, Features.TeamMembers,
                Features.CustomDomain, Features.PrioritySupport
            },
            [TenantPlan.Enterprise] = new HashSet<string>
            {
                Features.BasicApi, Features.Analytics, Features.TeamMembers,
                Features.CustomDomain, Features.PrioritySupport, Features.WhiteLabel
            }
        };

    private static readonly IReadOnlyDictionary<TenantPlan, IReadOnlyDictionary<string, int>> PlanLimits =
        new Dictionary<TenantPlan, IReadOnlyDictionary<string, int>>
        {
            [TenantPlan.Free] = new Dictionary<string, int>
            {
                [Features.Limits.MaxUsers]       = 1,
                [Features.Limits.MaxProjects]    = 3,
                [Features.Limits.MaxApiCallsDay] = 100,
                [Features.Limits.StorageGb]      = 1
            },
            [TenantPlan.Starter] = new Dictionary<string, int>
            {
                [Features.Limits.MaxUsers]       = 5,
                [Features.Limits.MaxProjects]    = 20,
                [Features.Limits.MaxApiCallsDay] = 1_000,
                [Features.Limits.StorageGb]      = 10
            },
            [TenantPlan.Professional] = new Dictionary<string, int>
            {
                [Features.Limits.MaxUsers]       = 25,
                [Features.Limits.MaxProjects]    = 100,
                [Features.Limits.MaxApiCallsDay] = 10_000,
                [Features.Limits.StorageGb]      = 100
            },
            [TenantPlan.Enterprise] = new Dictionary<string, int>
            {
                [Features.Limits.MaxUsers]       = int.MaxValue,
                [Features.Limits.MaxProjects]    = int.MaxValue,
                [Features.Limits.MaxApiCallsDay] = int.MaxValue,
                [Features.Limits.StorageGb]      = int.MaxValue
            }
        };

    public bool IsFeatureEnabled(TenantPlan plan, string featureName)
        => PlanFeatures.TryGetValue(plan, out var features) && features.Contains(featureName);

    public int GetLimit(TenantPlan plan, string limitName)
        => PlanLimits.TryGetValue(plan, out var limits) && limits.TryGetValue(limitName, out var value)
            ? value : 0;
}
