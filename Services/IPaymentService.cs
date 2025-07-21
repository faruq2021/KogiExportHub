using System.Threading.Tasks;
using KogiExportHub.Models;

namespace KogiExportHub.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> InitializePaymentAsync(decimal amount, string currency = "NGN", string customerEmail = "", string customerName = "");
        Task<PaymentResponse> InitializePaymentWithSplitAsync(decimal amount, string customerEmail, string customerName, List<CartItem> items);
        Task<PaymentVerificationResponse> VerifyPaymentAsync(string transactionId);
        Task<SubaccountResponse> CreateSubaccountAsync(string email, string accountNumber, string bankCode, string businessName);
    
        // New disbursement methods
        Task<TransferResponse> InitiateTransferAsync(decimal amount, string recipientAccount, string bankCode, string recipientName, string narration = "");
        Task<TransferResponse> VerifyTransferAsync(string transferId);
        Task<BankListResponse> GetBanksAsync();
        Task<AccountValidationResponse> ValidateAccountAsync(string accountNumber, string bankCode);
    }

    public class PaymentResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string PaymentLink { get; set; }
        public string TransactionId { get; set; }
    }

    public class PaymentVerificationResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool IsSuccessful { get; set; }
    }

    public class SubaccountResponse
    {
        public string Status { get; set; }
        public string SubaccountId { get; set; }
        public string Message { get; set; }
    }

    // New response models
    public class TransferResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string TransferId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
    }

    public class BankListResponse
    {
        public string Status { get; set; }
        public List<BankInfo> Banks { get; set; }
    }

    public class BankInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class AccountValidationResponse
    {
        public string Status { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
    }
}