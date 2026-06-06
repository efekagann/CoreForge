namespace CoreForge.Domain.Common;

public static class Features
{
    public const string BasicApi         = "basic_api";
    public const string Analytics        = "analytics";
    public const string TeamMembers      = "team_members";
    public const string CustomDomain     = "custom_domain";
    public const string PrioritySupport  = "priority_support";
    public const string WhiteLabel       = "white_label";

    public static class Limits
    {
        public const string MaxUsers        = "max_users";
        public const string MaxProjects     = "max_projects";
        public const string MaxApiCallsDay  = "max_api_calls_per_day";
        public const string StorageGb       = "storage_gb";
    }
}
