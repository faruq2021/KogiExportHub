using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;

namespace KogiExportHub.Controllers
{
    [Authorize]
    public class SellerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SellerDashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null)
            {
                return View(new SellerDashboardViewModel());
            }

            // Get seller's products
            var products = await _context.Products
                .Where(p => p.SellerId == userProfile.Id)
                .ToListAsync();

            // Get transactions for seller's products
            var transactions = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Where(t => t.Product.SellerId == userProfile.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var viewModel = new SellerDashboardViewModel
            {
                TotalProducts = products.Count,
                TotalSales = transactions.Where(t => t.Status == "Completed").Sum(t => t.TotalAmount),
                PendingOrders = transactions.Where(t => t.Status == "Pending").Count(),
                CompletedOrders = transactions.Where(t => t.Status == "Completed").Count(),
                RecentTransactions = transactions.Take(10).ToList(),
                MonthlyEarnings = transactions
                    .Where(t => t.Status == "Completed" && t.CreatedAt.Month == DateTime.Now.Month && t.CreatedAt.Year == DateTime.Now.Year)
                    .Sum(t => t.TotalAmount)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Sales()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null)
            {
                return View(new List<Transaction>());
            }

            var transactions = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Where(t => t.Product.SellerId == userProfile.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(transactions);
        }
    }
}