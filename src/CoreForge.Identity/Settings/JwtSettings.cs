namespace CoreForge.Identity.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = "CoreForge";
    public string Audience { get; init; } = "CoreForgeUsers";
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
