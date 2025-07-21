using KogiExportHub.Models;

namespace KogiExportHub.Services
{
    public interface ITaxService
    {
        Task<List<TaxCalculation>> CalculateTaxesAsync(Transaction transaction);
        Task<TaxReceipt> GenerateTaxReceiptAsync(Transaction transaction, List<TaxCalculation> taxCalculations);
        Task<List<TaxRule>> GetApplicableTaxRulesAsync(Transaction transaction);
        Task ProcessTaxesForTransactionAsync(Transaction transaction);
    }
}