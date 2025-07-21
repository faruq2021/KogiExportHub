using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;

namespace KogiExportHub.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TransactionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> History()
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
                .Where(t => t.BuyerId == userProfile.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(transactions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .FirstOrDefaultAsync(t => t.Id == id && t.BuyerId == userProfile.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}