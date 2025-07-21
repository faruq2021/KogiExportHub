using KogiExportHub.Models;

namespace KogiExportHub.Services
{
    public interface IReceiptService
    {
        Task<byte[]> GenerateReceiptPdfAsync(Transaction transaction);
        Task<string> GenerateReceiptHtmlAsync(Transaction transaction);
    }
}