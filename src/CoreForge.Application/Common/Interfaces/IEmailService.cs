namespace CoreForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default);
    Task SendTemplatedAsync(string to, string templateName, Dictionary<string, string> variables, CancellationToken ct = default);
}
