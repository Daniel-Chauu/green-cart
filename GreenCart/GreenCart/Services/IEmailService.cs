using System.Threading;
using System.Threading.Tasks;

namespace GreenCart.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
