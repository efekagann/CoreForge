using CoreForge.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CoreForge.Infrastructure.Email;

public class MailKitEmailService(IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default)
    {
        var message = BuildMessage(to, subject, body, isHtml);
        await SendAsync(message, ct);
    }

    public async Task SendTemplatedAsync(string to, string templateName, Dictionary<string, string> variables, CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync(templateName);
        var body = variables.Aggregate(template, (current, kv) =>
            current.Replace($"{{{{{kv.Key}}}}}", kv.Value));

        var subject = variables.TryGetValue("Subject", out var s) ? s : templateName;
        await SendAsync(to, subject, body, isHtml: true, ct);
    }

    private MimeMessage BuildMessage(string to, string subject, string body, bool isHtml)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.Smtp.FromName, _settings.Smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(isHtml ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
        {
            Text = body
        };
        return message;
    }

    private async Task SendAsync(MimeMessage message, CancellationToken ct)
    {
        using var client = new SmtpClient();
        var secureSocketOptions = _settings.Smtp.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, secureSocketOptions, ct);
        await client.AuthenticateAsync(_settings.Smtp.Username, _settings.Smtp.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }

    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Email", "Templates", $"{templateName}.html");
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path)
            : $"<p>{{{{Body}}}}</p>";
    }
}
