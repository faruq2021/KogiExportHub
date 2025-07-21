using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using KogiExportHub.Infrastructure;
using KogiExportHub.Services;

namespace KogiExportHub.Controllers
{
    [RoleAuthorization("Admin")]
    public class TaxationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxService _taxService;
        
        public TaxationController(ApplicationDbContext context, ITaxService taxService)
        {
            _context = context;
            _taxService = taxService;
        }
        
        // Government Revenue Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisYear = new DateTime(today.Year, 1, 1);
            
            var viewModel = new TaxationDashboardViewModel
            {
                TodayRevenue = await _context.GovernmentRevenues
                    .Where(gr => gr.RevenueDate.Date == today)
                    .SumAsync(gr => gr.Amount),
                    
                MonthlyRevenue = await _context.GovernmentRevenues
                    .Where(gr => gr.RevenueDate >= thisMonth)
                    .SumAsync(gr => gr.Amount),
                    
                YearlyRevenue = await _context.GovernmentRevenues
                    .Where(gr => gr.RevenueDate >= thisYear)
                    .SumAsync(gr => gr.Amount),
                    
                TotalRevenue = await _context.GovernmentRevenues
                    .SumAsync(gr => gr.Amount),
                    
                RevenueByCategory = await _context.GovernmentRevenues
                    .GroupBy(gr => gr.Category)
                    .Select(g => new RevenueCategoryViewModel
                    {
                        Category = g.Key,
                        Amount = g.Sum(gr => gr.Amount),
                        Count = g.Count()
                    })
                    .OrderByDescending(rc => rc.Amount)
                    .ToListAsync(),
                    
                RecentRevenues = await _context.GovernmentRevenues
                    .Include(gr => gr.Transaction)
                    .ThenInclude(t => t.Product)
                    .OrderByDescending(gr => gr.CreatedAt)
                    .Take(10)
                    .ToListAsync(),
                    
                MonthlyTrend = await GetMonthlyRevenueTrendAsync()
            };
            
            return View(viewModel);
        }
        
        // Tax Rules Management
        public async Task<IActionResult> TaxRules()
        {
            var taxRules = await _context.TaxRules
                .OrderByDescending(tr => tr.CreatedAt)
                .ToListAsync();
            return View(taxRules);
        }
        
        [HttpGet]
        public IActionResult CreateTaxRule()
        {
            return View(new TaxRule { EffectiveDate = DateTime.Today });
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateTaxRule(TaxRule taxRule)
        {
            if (ModelState.IsValid)
            {
                taxRule.CreatedAt = DateTime.Now;
                taxRule.UpdatedAt = DateTime.Now;
                
                _context.TaxRules.Add(taxRule);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Tax rule created successfully!";
                return RedirectToAction(nameof(TaxRules));
            }
            
            return View(taxRule);
        }
        
        [HttpGet]
        public async Task<IActionResult> EditTaxRule(int id)
        {
            var taxRule = await _context.TaxRules.FindAsync(id);
            if (taxRule == null)
            {
                return NotFound();
            }
            
            return View(taxRule);
        }
        
        [HttpPost]
        public async Task<IActionResult> EditTaxRule(int id, TaxRule taxRule)
        {
            if (id != taxRule.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    taxRule.UpdatedAt = DateTime.Now;
                    _context.Update(taxRule);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Tax rule updated successfully!";
                    return RedirectToAction(nameof(TaxRules));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TaxRuleExistsAsync(taxRule.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            
            return View(taxRule);
        }
        
        // Revenue Reports
        public async Task<IActionResult> RevenueReports(DateTime? startDate, DateTime? endDate, string category)
        {
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today;
            
            var query = _context.GovernmentRevenues
                .Include(gr => gr.Transaction)
                .ThenInclude(t => t.Product)
                .Where(gr => gr.RevenueDate.Date >= startDate.Value.Date && 
                           gr.RevenueDate.Date <= endDate.Value.Date);
                           
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(gr => gr.Category == category);
            }
            
            var revenues = await query
                .OrderByDescending(gr => gr.RevenueDate)
                .ToListAsync();
                
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Category = category;
            ViewBag.Categories = await _context.GovernmentRevenues
                .Select(gr => gr.Category)
                .Distinct()
                .ToListAsync();
                
            return View(revenues);
        }
        
        // Tax Receipt Download
        public async Task<IActionResult> DownloadTaxReceipt(int transactionId)
        {
            var receipt = await _context.TaxReceipts
                .Include(tr => tr.Transaction)
                .ThenInclude(t => t.Product)
                .Include(tr => tr.Payer)
                .FirstOrDefaultAsync(tr => tr.TransactionId == transactionId);
                
            if (receipt == null)
            {
                return NotFound();
            }
            
            var receiptHtml = GenerateTaxReceiptHtml(receipt);
            var receiptBytes = System.Text.Encoding.UTF8.GetBytes(receiptHtml);
            
            return File(receiptBytes, "text/html", $"TaxReceipt_{receipt.ReceiptNumber}.html");
        }
        
        private async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueTrendAsync()
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-6);
            
            return await _context.GovernmentRevenues
                .Where(gr => gr.RevenueDate >= sixMonthsAgo)
                .GroupBy(gr => new { gr.RevenueDate.Year, gr.RevenueDate.Month })
                .Select(g => new MonthlyRevenueViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(gr => gr.Amount)
                })
                .OrderBy(mr => mr.Year)
                .ThenBy(mr => mr.Month)
                .ToListAsync();
        }
        
        private async Task<bool> TaxRuleExistsAsync(int id)
        {
            return await _context.TaxRules.AnyAsync(e => e.Id == id);
        }
        
        private string GenerateTaxReceiptHtml(TaxReceipt receipt)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Tax Receipt - {receipt.ReceiptNumber}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .receipt-info {{ margin-bottom: 20px; }}
        .tax-details {{ margin-top: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Kogi State Government</h1>
        <h2>Tax Receipt</h2>
        <p>Receipt Number: {receipt.ReceiptNumber}</p>
    </div>
    
    <div class=""receipt-info"">
        <p><strong>Issued Date:</strong> {receipt.IssuedDate:yyyy-MM-dd HH:mm}</p>
        <p><strong>Payer:</strong> {receipt.PayerName}</p>
        <p><strong>Email:</strong> {receipt.PayerEmail}</p>
        <p><strong>Transaction Amount:</strong> ₦{receipt.TransactionAmount:N2}</p>
        <p><strong>Total Tax Amount:</strong> ₦{receipt.TotalTaxAmount:N2}</p>
    </div>
    
    <div class=""tax-details"">
        <h3>Tax Breakdown</h3>
        <p>{receipt.TaxBreakdown}</p>
    </div>
    
    <div style=""margin-top: 40px; text-align: center;"">
        <p><em>This is an automatically generated tax receipt.</em></p>
    </div>
</body>
</html>";
        }
    }
}