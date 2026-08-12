using System;
using System.Threading;
using System.Threading.Tasks;
using GreenCart.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GreenCart.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Connect to Gmail SMTP with StartTLS on port 587
                await client.ConnectAsync(
                    _smtpSettings.Host,
                    _smtpSettings.Port,
                    _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                    cancellationToken);

                // Authenticate with Gmail App Password
                await client.AuthenticateAsync(
                    _smtpSettings.Username,
                    _smtpSettings.Password,
                    cancellationToken);

                // Send
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
                // Do NOT rethrow – caller handles gracefully
                throw;
            }
        }
    }
}
