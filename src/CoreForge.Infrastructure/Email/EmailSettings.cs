namespace CoreForge.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";
    public string Provider { get; set; } = "Mock";

    public SmtpSettings Smtp { get; set; } = new();
}

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.example.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@example.com";
    public string FromName { get; set; } = "CoreForge";
    public bool UseSsl { get; set; } = true;
}
