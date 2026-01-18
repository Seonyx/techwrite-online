using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

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
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = $"[TechWrite Contact] {subject}",
                IsBodyHtml = true,
                Body = BuildEmailBody(name, email, subject, message)
            };
            mailMessage.To.Add(_settings.ToAddress);
            mailMessage.ReplyToList.Add(new MailAddress(email, name));

            await client.SendMailAsync(mailMessage);

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
