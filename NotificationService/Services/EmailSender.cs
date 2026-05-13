using MailKit.Net.Smtp;
using MimeKit;

namespace NotificationService.Services;

public interface IEmailSender
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
}
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var portStr = smtp["Port"];
            var user = smtp["User"];
            var pass = smtp["Password"];
            var from = smtp["From"] ?? "noreply@spendsmart.local";

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr))
            {
                _logger.LogInformation("SMTP not configured — skipping email to {To}", toEmail);
                return false;
            }

            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(from));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, int.Parse(portStr), MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
            if (!string.IsNullOrEmpty(user))
                await client.AuthenticateAsync(user, pass);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed to {To}", toEmail);
            return false;
        }
    }
}
