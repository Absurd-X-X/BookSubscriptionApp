using Application.Services;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Sockets;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public Task<EmailResult> SendEmailAsync(string to, string subject, string body)
        {
            return SendEmailAsync(new List<string> { to }, subject, body);
        }

        public async Task<EmailResult> SendEmailAsync(List<string> to, string subject, string body)
        {
            var email = new MimeMessage();

            // From
            email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));

            // To
            foreach (var recipient in to)
                email.To.Add(MailboxAddress.Parse(recipient));

            email.Subject = subject;

            // HTML body
            email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.From, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return EmailResult.Ok();
            }
            catch (SocketException)
            {
                // No network / DNS failure / host unreachable
                return EmailResult.Fail("Unable to reach the mail server. Please check your network connection.");
            }
            catch (SmtpCommandException ex)
            {
                // Server reachable but rejected something (auth, recipient, etc.)
                return EmailResult.Fail($"The mail server rejected the request: {ex.Message}");
            }
            catch (SmtpProtocolException)
            {
                // Connection dropped mid-protocol
                return EmailResult.Fail("Connection to the mail server was lost. Please try again.");
            }
            catch (Exception ex)
            {
                // Catch-all so a bad email never crashes the caller
                return EmailResult.Fail($"Failed to send email: {ex.Message}");
            }
        }
    }

}
