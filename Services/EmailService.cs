using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using KogiExportHub.Models;

namespace KogiExportHub.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // For development, just log the email
                _logger.LogInformation($"Email would be sent to {email} with subject: {subject}");
                _logger.LogInformation($"Message: {message}");
                
                // TODO: Implement actual email sending with SMTP
                // var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
                // {
                //     Port = int.Parse(_configuration["Email:Port"]),
                //     Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
                //     EnableSsl = true,
                // };
                // 
                // var mailMessage = new MailMessage
                // {
                //     From = new MailAddress(_configuration["Email:FromAddress"]),
                //     Subject = subject,
                //     Body = message,
                //     IsBodyHtml = true,
                // };
                // mailMessage.To.Add(email);
                // 
                // await smtpClient.SendMailAsync(mailMessage);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                throw;
            }
        }

        public async Task SendPaymentConfirmationAsync(Transaction transaction, UserProfile buyer)
        {
            var subject = "Payment Confirmation - KogiExportHub";
            var message = $@"
            <h2>Payment Confirmation</h2>
            <p>Dear {buyer.FirstName} {buyer.LastName},</p>
            <p>Thank you for your purchase! Your payment has been successfully processed.</p>
            
            <h3>Transaction Details:</h3>
            <ul>
                <li><strong>Transaction ID:</strong> {transaction.Id}</li>
                <li><strong>Reference:</strong> {transaction.TransactionReference}</li>
                <li><strong>Product:</strong> {transaction.Product?.Name}</li>
                <li><strong>Quantity:</strong> {transaction.Quantity}</li>
                <li><strong>Total Amount:</strong> ₦{transaction.TotalAmount:N2}</li>
                <li><strong>Date:</strong> {transaction.CreatedAt:MMMM dd, yyyy 'at' hh:mm tt}</li>
            </ul>
            
            <p>You can view your transaction history and download your receipt by logging into your account.</p>
            
            <p>Thank you for choosing KogiExportHub!</p>
            
            <p>Best regards,<br>The KogiExportHub Team</p>
            ";

            await SendEmailAsync(buyer.Email, subject, message);
        }
    }
}