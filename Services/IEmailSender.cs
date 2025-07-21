using System.Threading.Tasks;

namespace KogiExportHub.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}