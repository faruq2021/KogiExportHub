using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KogiExportHub.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation($"Email sent to {email} with subject: {subject}");
            
            // TODO: Implement actual email sending logic here
            // For now, just return a completed task
            return Task.CompletedTask;
        }
    }
}