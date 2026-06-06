namespace CoreForge.Domain.Common;

public enum SubscriptionStatus
{
    Trialing = 0,
    Active    = 1,
    Cancelled = 2,
    Expired   = 3,
    PastDue   = 4
}
