using KogiExportHub.Models;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using System.Text;

namespace KogiExportHub.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ApplicationDbContext _context;

        public ReceiptService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateReceiptHtmlAsync(Transaction transaction)
        {
            var transactionWithDetails = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);

            if (transactionWithDetails == null)
                throw new ArgumentException("Transaction not found");

            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Payment Receipt</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .receipt-details {{ margin-bottom: 20px; }}
                    .receipt-table {{ width: 100%; border-collapse: collapse; }}
                    .receipt-table th, .receipt-table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    .receipt-table th {{ background-color: #f2f2f2; }}
                    .total {{ font-weight: bold; font-size: 18px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>KogiExportHub</h1>
                    <h2>Payment Receipt</h2>
                </div>
                
                <div class='receipt-details'>
                    <p><strong>Receipt #:</strong> {transactionWithDetails.Id}</p>
                    <p><strong>Transaction Reference:</strong> {transactionWithDetails.TransactionReference}</p>
                    <p><strong>Date:</strong> {transactionWithDetails.CreatedAt:MMMM dd, yyyy 'at' hh:mm tt}</p>
                    <p><strong>Customer:</strong> {transactionWithDetails.Buyer?.FirstName} {transactionWithDetails.Buyer?.LastName}</p>
                    <p><strong>Email:</strong> {transactionWithDetails.Buyer?.Email}</p>
                </div>
                
                <table class='receipt-table'>
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Unit Price</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{transactionWithDetails.Product?.Name}</td>
                            <td>{transactionWithDetails.Quantity}</td>
                            <td>₦{(transactionWithDetails.TotalAmount / transactionWithDetails.Quantity):N2}</td>
                            <td>₦{transactionWithDetails.TotalAmount:N2}</td>
                        </tr>
                    </tbody>
                </table>
                
                <div style='margin-top: 20px; text-align: right;'>
                    <p class='total'>Total Amount: ₦{transactionWithDetails.TotalAmount:N2}</p>
                    <p><strong>Payment Method:</strong> {transactionWithDetails.PaymentMethod}</p>
                    <p><strong>Status:</strong> {transactionWithDetails.Status}</p>
                </div>
                
                <div style='margin-top: 30px; text-align: center; font-size: 12px; color: #666;'>
                    <p>Thank you for your business!</p>
                    <p>For any inquiries, please contact us at support@kogiexporthub.com</p>
                </div>
            </body>
            </html>";

            return html;
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(Transaction transaction)
        {
            var html = await GenerateReceiptHtmlAsync(transaction);
            // For now, return HTML as bytes. You can integrate a PDF library like iTextSharp later
            return Encoding.UTF8.GetBytes(html);
        }
    }
}