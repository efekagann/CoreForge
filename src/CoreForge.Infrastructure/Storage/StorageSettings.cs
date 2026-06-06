namespace CoreForge.Infrastructure.Storage;

public class StorageSettings
{
    public const string SectionName = "Storage";
    public string Provider { get; set; } = "Local";
    public LocalStorageOptions Local { get; set; } = new();
}

public class LocalStorageOptions
{
    public string BasePath { get; set; } = "uploads";
}
