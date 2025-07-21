using KogiExportHub.Data;
using KogiExportHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KogiExportHub.Services
{
    public class TaxService : ITaxService
    {
        private readonly ApplicationDbContext _context;
        
        public TaxService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<TaxRule>> GetApplicableTaxRulesAsync(Transaction transaction)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.Id == transaction.ProductId);
                
            var currentDate = DateTime.Now;
            
            return await _context.TaxRules
                .Where(tr => tr.IsActive && 
                           tr.EffectiveDate <= currentDate &&
                           (tr.ExpiryDate == null || tr.ExpiryDate >= currentDate) &&
                           (tr.ApplicableCategory == null || tr.ApplicableCategory == product.Category.Name) &&
                           (tr.ApplicableLocation == null || tr.ApplicableLocation == product.Location.Name) &&
                           transaction.TotalAmount >= tr.MinAmount &&
                           (tr.MaxAmount == 0 || transaction.TotalAmount <= tr.MaxAmount))
                .ToListAsync();
        }
        
        public async Task<List<TaxCalculation>> CalculateTaxesAsync(Transaction transaction)
        {
            var applicableRules = await GetApplicableTaxRulesAsync(transaction);
            var taxCalculations = new List<TaxCalculation>();
            
            foreach (var rule in applicableRules)
            {
                var taxAmount = (transaction.TotalAmount * rule.Rate) / 100;
                
                var taxCalculation = new TaxCalculation
                {
                    TransactionId = transaction.Id,
                    TaxRuleId = rule.Id,
                    BaseAmount = transaction.TotalAmount,
                    TaxRate = rule.Rate,
                    TaxAmount = taxAmount,
                    TaxType = rule.TaxType,
                    CalculatedAt = DateTime.Now
                };
                
                taxCalculations.Add(taxCalculation);
            }
            
            return taxCalculations;
        }
        
        public async Task<TaxReceipt> GenerateTaxReceiptAsync(Transaction transaction, List<TaxCalculation> taxCalculations)
        {
            var buyer = await _context.UserProfiles.FindAsync(transaction.BuyerId);
            var totalTax = taxCalculations.Sum(tc => tc.TaxAmount);
            
            var taxBreakdown = taxCalculations.Select(tc => new
            {
                TaxType = tc.TaxType,
                Rate = tc.TaxRate,
                BaseAmount = tc.BaseAmount,
                TaxAmount = tc.TaxAmount
            });
            
            var receipt = new TaxReceipt
            {
                ReceiptNumber = $"TR-{DateTime.Now:yyyyMMdd}-{transaction.Id:D6}",
                TransactionId = transaction.Id,
                PayerId = transaction.BuyerId,
                TotalTaxAmount = totalTax,
                TransactionAmount = transaction.TotalAmount,
                IssuedDate = DateTime.Now,
                PayerName = $"{buyer?.FirstName} {buyer?.LastName}",
                PayerEmail = buyer?.Email,
                TaxBreakdown = JsonSerializer.Serialize(taxBreakdown),
                Status = "Issued",
                CreatedAt = DateTime.Now
            };
            
            return receipt;
        }
        
        public async Task ProcessTaxesForTransactionAsync(Transaction transaction)
        {
            var taxCalculations = await CalculateTaxesAsync(transaction);
            
            if (taxCalculations.Any())
            {
                // Save tax calculations
                _context.TaxCalculations.AddRange(taxCalculations);
                await _context.SaveChangesAsync();
                
                // Generate government revenue entries
                foreach (var taxCalc in taxCalculations)
                {
                    var revenue = new GovernmentRevenue
                    {
                        RevenueDate = DateTime.Now,
                        RevenueType = "Tax",
                        Source = "Transaction",
                        Amount = taxCalc.TaxAmount,
                        Category = taxCalc.TaxType,
                        Location = "Kogi State",
                        TransactionId = transaction.Id,
                        TaxCalculationId = taxCalc.Id,
                        Description = $"{taxCalc.TaxType} tax from transaction {transaction.Id}",
                        ReferenceNumber = $"REV-{DateTime.Now:yyyyMMdd}-{taxCalc.Id:D6}",
                        CreatedAt = DateTime.Now
                    };
                    
                    _context.GovernmentRevenues.Add(revenue);
                }
                
                // Generate tax receipt
                var receipt = await GenerateTaxReceiptAsync(transaction, taxCalculations);
                _context.TaxReceipts.Add(receipt);
                
                await _context.SaveChangesAsync();
            }
        }
    }
}