using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using Microsoft.AspNetCore.Identity;

namespace KogiExportHub.Controllers
{
    public class InvestorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvestorController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Investor Portal Landing Page
        public async Task<IActionResult> Index()
        {
            var opportunities = await _context.InvestmentOpportunities
                .Include(o => o.Location)
                .Include(o => o.LocalPartner)
                .Where(o => o.Status == "Open")
                .OrderByDescending(o => o.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.TotalOpportunities = await _context.InvestmentOpportunities.CountAsync(o => o.Status == "Open");
            ViewBag.ActiveInvestors = await _context.Investors.CountAsync(i => i.IsActive && i.VerificationStatus == "Verified");
            ViewBag.TotalInvestment = await _context.JointVentures.SumAsync(jv => jv.TotalInvestment);

            return View(opportunities);
        }

        // GET: Investor Registration
        public IActionResult Register()
        {
            return View();
        }

        // POST: Investor Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Investor investor, IFormFile? verificationDocument)
        {
            if (ModelState.IsValid)
            {
                // Handle document upload
                if (verificationDocument != null && verificationDocument.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "investor-docs");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + verificationDocument.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await verificationDocument.CopyToAsync(fileStream);
                    }
                    
                    investor.VerificationDocuments = "/uploads/investor-docs/" + uniqueFileName;
                }

                investor.RegistrationDate = DateTime.Now;
                investor.VerificationStatus = "Pending";
                investor.IsActive = true;

                _context.Investors.Add(investor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Your application is under review. You will be notified once verified.";
                return RedirectToAction("RegistrationSuccess", new { id = investor.Id });
            }

            return View(investor);
        }

        // GET: Registration Success
        public async Task<IActionResult> RegistrationSuccess(int id)
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }

            return View(investor);
        }

        // GET: Investment Opportunities
        public async Task<IActionResult> Opportunities(string? sector, string? location, decimal? minInvestment, decimal? maxInvestment)
        {
            var query = _context.InvestmentOpportunities
                .Include(o => o.Location)
                .Include(o => o.LocalPartner)
                .Where(o => o.Status == "Open");

            if (!string.IsNullOrEmpty(sector))
            {
                query = query.Where(o => o.Sector.Contains(sector));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(o => o.Location.Name.Contains(location));
            }

            if (minInvestment.HasValue)
            {
                query = query.Where(o => o.RequiredInvestment >= minInvestment.Value);
            }

            if (maxInvestment.HasValue)
            {
                query = query.Where(o => o.RequiredInvestment <= maxInvestment.Value);
            }

            var opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            ViewBag.Sectors = await _context.InvestmentOpportunities
                .Select(o => o.Sector)
                .Distinct()
                .ToListAsync();

            ViewBag.Locations = await _context.Locations.ToListAsync();

            return View(opportunities);
        }

        // GET: Opportunity Details
        public async Task<IActionResult> OpportunityDetails(int id)
        {
            var opportunity = await _context.InvestmentOpportunities
                .Include(o => o.Location)
                .Include(o => o.LocalPartner)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        // GET: Investor Dashboard (requires verification)
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.Email == user.Email);

            if (investor == null || investor.VerificationStatus != "Verified")
            {
                TempData["ErrorMessage"] = "Access denied. Please complete investor verification first.";
                return RedirectToAction("Index");
            }

            var jointVentures = await _context.JointVentures
                .Include(jv => jv.LocalPartner)
                .Include(jv => jv.Opportunity)
                .Where(jv => jv.InvestorId == investor.Id)
                .ToListAsync();

            var messages = await _context.InvestorMessages
                .Where(m => m.RecipientId == investor.Id)
                .OrderByDescending(m => m.SentAt)
                .Take(5)
                .ToListAsync();

            ViewBag.Investor = investor;
            ViewBag.JointVentures = jointVentures;
            ViewBag.RecentMessages = messages;

            return View();
        }

        // GET: Express Interest in Opportunity
        [Authorize]
        public async Task<IActionResult> ExpressInterest(int opportunityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.Email == user.Email && i.VerificationStatus == "Verified");

            if (investor == null)
            {
                TempData["ErrorMessage"] = "Please complete investor verification first.";
                return RedirectToAction("Register");
            }

            var opportunity = await _context.InvestmentOpportunities
                .Include(o => o.LocalPartner)
                .FirstOrDefaultAsync(o => o.Id == opportunityId);

            if (opportunity == null)
            {
                return NotFound();
            }

            ViewBag.Investor = investor;
            ViewBag.Opportunity = opportunity;

            return View();
        }

        // POST: Submit Interest
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInterest(int opportunityId, string message, decimal proposedInvestment)
        {
            var user = await _userManager.GetUserAsync(User);
            var investor = await _context.Investors
                .FirstOrDefaultAsync(i => i.Email == user.Email && i.VerificationStatus == "Verified");

            var opportunity = await _context.InvestmentOpportunities
                .Include(o => o.LocalPartner)
                .FirstOrDefaultAsync(o => o.Id == opportunityId);

            if (investor == null || opportunity == null)
            {
                return BadRequest();
            }

            // Create message to local partner
            var investorMessage = new InvestorMessage
            {
                SenderId = investor.Id,
                LocalUserId = opportunity.LocalPartnerId,
                Subject = $"Investment Interest: {opportunity.Title}",
                Content = $"Investor {investor.CompanyName} has expressed interest in your opportunity '{opportunity.Title}' with a proposed investment of ₦{proposedInvestment:N2}.\n\nMessage: {message}",
                SentAt = DateTime.Now
            };

            _context.InvestorMessages.Add(investorMessage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your interest has been submitted successfully. The local partner will be notified.";
            return RedirectToAction("OpportunityDetails", new { id = opportunityId });
        }
    }
}