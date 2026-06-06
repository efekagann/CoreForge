using CoreForge.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoreForge.Infrastructure.Email;

public class MockEmailService(ILogger<MockEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default)
    {
        logger.LogInformation("[MockEmail] To: {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplatedAsync(string to, string templateName, Dictionary<string, string> variables, CancellationToken ct = default)
    {
        logger.LogInformation("[MockEmail] To: {To} | Template: {Template} | Variables: {Vars}",
            to, templateName, string.Join(", ", variables.Select(kv => $"{kv.Key}={kv.Value}")));
        return Task.CompletedTask;
    }
}
