using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using Microsoft.AspNetCore.Identity;

namespace KogiExportHub.Controllers
{
    [Authorize]
    public class InvestmentOpportunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvestmentOpportunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: My Opportunities
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
                TempData["ErrorMessage"] = "Please complete your profile first.";
                return RedirectToAction("Profile", "Account");
            }

            var opportunities = await _context.InvestmentOpportunities
                .Include(o => o.Location)
                .Include(o => o.Investor)
                .Where(o => o.LocalPartnerId == userProfile.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(opportunities);
        }

        // GET: Create Opportunity
        public async Task<IActionResult> Create()
        {
            ViewBag.Locations = await _context.Locations.ToListAsync();
            return View();
        }

        // POST: Create Opportunity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestmentOpportunity opportunity, IFormFile? businessPlan, IFormFile? financialProjections)
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null)
            {
                TempData["ErrorMessage"] = "Please complete your profile first.";
                return RedirectToAction("Profile", "Account");
            }

            try
            {
                // Handle file uploads
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "opportunities");
                Directory.CreateDirectory(uploadsFolder);

                if (businessPlan != null && businessPlan.Length > 0)
                {
                    var businessPlanFileName = Guid.NewGuid().ToString() + "_" + businessPlan.FileName;
                    var businessPlanPath = Path.Combine(uploadsFolder, businessPlanFileName);
                    
                    using (var fileStream = new FileStream(businessPlanPath, FileMode.Create))
                    {
                        await businessPlan.CopyToAsync(fileStream);
                    }
                    
                    opportunity.BusinessPlan = "/uploads/opportunities/" + businessPlanFileName;
                }

                if (financialProjections != null && financialProjections.Length > 0)
                {
                    var financialFileName = Guid.NewGuid().ToString() + "_" + financialProjections.FileName;
                    var financialPath = Path.Combine(uploadsFolder, financialFileName);
                    
                    using (var fileStream = new FileStream(financialPath, FileMode.Create))
                    {
                        await financialProjections.CopyToAsync(fileStream);
                    }
                    
                    opportunity.FinancialProjections = "/uploads/opportunities/" + financialFileName;
                }

                opportunity.LocalPartnerId = userProfile.Id;
                opportunity.Status = "Open";
                opportunity.CreatedAt = DateTime.Now;
                opportunity.UpdatedAt = DateTime.Now;

                _context.InvestmentOpportunities.Add(opportunity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Investment opportunity created successfully!";
                return RedirectToAction("Details", new { id = opportunity.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the opportunity. Please try again.";
                ViewBag.Locations = await _context.Locations.ToListAsync();
                return View(opportunity);
            }
        }

        // GET: Opportunity Details
        public async Task<IActionResult> Details(int id)
        {
            var opportunity = await _context.InvestmentOpportunities
                .Include(o => o.Location)
                .Include(o => o.LocalPartner)
                .Include(o => o.Investor)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
            {
                return NotFound();
            }

            // Get interested investors (messages)
            var interestedInvestors = await _context.InvestorMessages
                .Include(m => m.Sender)
                .Where(m => m.LocalUserId == opportunity.LocalPartnerId && 
                           m.Subject.Contains(opportunity.Title))
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            ViewBag.InterestedInvestors = interestedInvestors;

            return View(opportunity);
        }

        // GET: Edit Opportunity
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            var opportunity = await _context.InvestmentOpportunities
                .FirstOrDefaultAsync(o => o.Id == id && o.LocalPartnerId == userProfile.Id);

            if (opportunity == null)
            {
                return NotFound();
            }

            ViewBag.Locations = await _context.Locations.ToListAsync();
            return View(opportunity);
        }

        // POST: Edit Opportunity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvestmentOpportunity opportunity)
        {
            if (id != opportunity.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            var existingOpportunity = await _context.InvestmentOpportunities
                .FirstOrDefaultAsync(o => o.Id == id && o.LocalPartnerId == userProfile.Id);

            if (existingOpportunity == null)
            {
                return NotFound();
            }

            // Only update fields that have been provided (not null/empty)
            if (!string.IsNullOrWhiteSpace(opportunity.Title))
                existingOpportunity.Title = opportunity.Title;
            
            if (!string.IsNullOrWhiteSpace(opportunity.Description))
                existingOpportunity.Description = opportunity.Description;
            
            if (!string.IsNullOrWhiteSpace(opportunity.Sector))
                existingOpportunity.Sector = opportunity.Sector;
            
            if (opportunity.RequiredInvestment > 0)
                existingOpportunity.RequiredInvestment = opportunity.RequiredInvestment;
            
            if (opportunity.ExpectedROI > 0)
                existingOpportunity.ExpectedROI = opportunity.ExpectedROI;
            
            if (opportunity.InvestmentPeriodMonths > 0)
                existingOpportunity.InvestmentPeriodMonths = opportunity.InvestmentPeriodMonths;
            
            if (!string.IsNullOrWhiteSpace(opportunity.RiskLevel))
                existingOpportunity.RiskLevel = opportunity.RiskLevel;
            
            if (opportunity.LocationId > 0)
                existingOpportunity.LocationId = opportunity.LocationId;
            
            existingOpportunity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Opportunity updated successfully!";
            return RedirectToAction("Details", new { id = opportunity.Id });
        }
    }
}