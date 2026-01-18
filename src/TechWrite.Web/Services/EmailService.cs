using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TechWrite.Web.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendContactEmailAsync(string name, string email, string subject, string message)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(_settings.ToAddress));
            mimeMessage.ReplyTo.Add(new MailboxAddress(name, email));
            mimeMessage.Subject = $"[TechWrite Contact] {subject}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildEmailBody(name, email, subject, message)
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect with STARTTLS on port 587
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Authenticate
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Contact email sent successfully from {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact email from {Email}", email);
            return false;
        }
    }

    private static string BuildEmailBody(string name, string email, string subject, string message)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #212529; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; }}
        .field {{ margin-bottom: 15px; }}
        .label {{ font-weight: bold; color: #495057; }}
        .message {{ background-color: white; padding: 15px; border: 1px solid #dee2e6; margin-top: 10px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Contact Form Submission</h1>
        </div>
        <div class='content'>
            <div class='field'>
                <span class='label'>From:</span> {WebUtility.HtmlEncode(name)}
            </div>
            <div class='field'>
                <span class='label'>Email:</span> <a href='mailto:{WebUtility.HtmlEncode(email)}'>{WebUtility.HtmlEncode(email)}</a>
            </div>
            <div class='field'>
                <span class='label'>Subject:</span> {WebUtility.HtmlEncode(subject)}
            </div>
            <div class='field'>
                <span class='label'>Message:</span>
                <div class='message'>{WebUtility.HtmlEncode(message).Replace("\n", "<br>")}</div>
            </div>
        </div>
        <div class='footer'>
            <p>This message was sent via the contact form on TechWrite.Online</p>
            <p>You can reply directly to this email to respond to {WebUtility.HtmlEncode(name)}.</p>
        </div>
    </div>
</body>
</html>";
    }
}
