using Infrastructure.Services;

namespace Application.Services
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(string to, string subject, string body);
        Task<EmailResult> SendEmailAsync(List<string> to, string subject, string body);
    }
}
