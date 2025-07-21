using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using KogiExportHub.Infrastructure;

namespace KogiExportHub.Controllers
{
    [RoleAuthorization("Admin", "Government")]
    public class MiningController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MiningController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Miners Management
        public async Task<IActionResult> Index()
        {
            var miners = await _context.Miners
                .Include(m => m.Location)
                .OrderByDescending(m => m.RegistrationDate)
                .ToListAsync();
            return View(miners);
        }

        public async Task<IActionResult> MinerDetails(int id)
        {
            var miner = await _context.Miners
                .Include(m => m.Location)
                .Include(m => m.MiningActivities)
                .Include(m => m.EnvironmentalCompliances)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (miner == null)
            {
                return NotFound();
            }

            return View(miner);
        }

        public async Task<IActionResult> CreateMiner()
        {
            ViewBag.Locations = await _context.Locations.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMiner(Miner miner)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(miner.MiningType))
            {
                ModelState.AddModelError("MiningType", "Mining Type is required.");
            }
            
            if (string.IsNullOrEmpty(miner.FullName))
            {
                ModelState.AddModelError("FullName", "Full Name is required.");
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.Locations = await _context.Locations.ToListAsync();
                return View(miner);
            }

            // Generate license number
            miner.LicenseNumber = $"KG-MIN-{DateTime.Now.Year}-{(await _context.Miners.CountAsync() + 1):D4}";

            _context.Miners.Add(miner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Miner registered successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditMiner(int id)
        {
            var miner = await _context.Miners.FindAsync(id);
            if (miner == null)
            {
                return NotFound();
            }

            ViewBag.Locations = await _context.Locations.ToListAsync();
            return View(miner);
        }

        [HttpPost]
        public async Task<IActionResult> EditMiner(Miner miner)
        {
            var existingMiner = await _context.Miners.FindAsync(miner.Id);
            if (existingMiner == null)
            {
                return NotFound();
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(miner.FullName))
                existingMiner.FullName = miner.FullName;
            if (!string.IsNullOrEmpty(miner.PhoneNumber))
                existingMiner.PhoneNumber = miner.PhoneNumber;
            if (!string.IsNullOrEmpty(miner.Email))
                existingMiner.Email = miner.Email;
            if (!string.IsNullOrEmpty(miner.Address))
                existingMiner.Address = miner.Address;
            if (!string.IsNullOrEmpty(miner.MiningType))
                existingMiner.MiningType = miner.MiningType;
            if (!string.IsNullOrEmpty(miner.LicenseStatus))
                existingMiner.LicenseStatus = miner.LicenseStatus;
            if (miner.IssueDate.HasValue)
                existingMiner.IssueDate = miner.IssueDate;
            if (miner.ExpiryDate.HasValue)
                existingMiner.ExpiryDate = miner.ExpiryDate;
            if (miner.LocationId.HasValue)
                existingMiner.LocationId = miner.LocationId;
            if (!string.IsNullOrEmpty(miner.Notes))
                existingMiner.Notes = miner.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Miner information updated successfully!";
            return RedirectToAction(nameof(MinerDetails), new { id = miner.Id });
        }

        // Mining Activities Management
        public async Task<IActionResult> Activities()
        {
            var activities = await _context.MiningActivities
                .Include(a => a.Miner)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
            return View(activities);
        }

        public async Task<IActionResult> CreateActivity()
        {
            ViewBag.Miners = await _context.Miners.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity(MiningActivity activity)
        {
            _context.MiningActivities.Add(activity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mining activity logged successfully!";
            return RedirectToAction(nameof(Activities));
        }

        // Environmental Compliance Management
        public async Task<IActionResult> Compliance()
        {
            var compliances = await _context.EnvironmentalCompliances
                .Include(c => c.Miner)
                .OrderByDescending(c => c.AssessmentDate)
                .ToListAsync();
            return View(compliances);
        }

        public async Task<IActionResult> CreateCompliance()
        {
            ViewBag.Miners = await _context.Miners.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompliance(EnvironmentalCompliance compliance)
        {
            _context.EnvironmentalCompliances.Add(compliance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Environmental compliance record created successfully!";
            return RedirectToAction(nameof(Compliance));
        }

        public async Task<IActionResult> ComplianceDetails(int id)
        {
            var compliance = await _context.EnvironmentalCompliances
                .Include(c => c.Miner)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compliance == null)
            {
                return NotFound();
            }

            return View(compliance);
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new MiningDashboardViewModel
            {
                TotalMiners = await _context.Miners.CountAsync(),
                ActiveMiners = await _context.Miners.CountAsync(m => m.LicenseStatus == "Active"),
                ActiveLicenses = await _context.Miners.CountAsync(m => m.LicenseStatus == "Active"),
                PendingLicenses = await _context.Miners.CountAsync(m => m.LicenseStatus == "Pending"),
                PendingApplications = await _context.Miners.CountAsync(m => m.LicenseStatus == "Pending"),
                TotalActivities = await _context.MiningActivities.CountAsync(),
                ComplianceIssues = await _context.EnvironmentalCompliances.CountAsync(c => c.ComplianceStatus == "Non-Compliant"),
                
                RecentMiners = await _context.Miners
                    .Include(m => m.Location)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentActivities = await _context.MiningActivities
                    .Include(ma => ma.Miner)
                    .OrderByDescending(ma => ma.ActivityDate)
                    .Take(10)
                    .ToListAsync(),
                    
                RecentCompliance = await _context.EnvironmentalCompliances
                    .Include(ec => ec.Miner)
                    .OrderByDescending(ec => ec.InspectionDate)
                    .Take(5)
                    .ToListAsync(),
                    
                MinersByStatus = await _context.Miners
                    .GroupBy(m => m.LicenseStatus)
                    .Select(g => new MinerStatusViewModel
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(),
                    
                ActivitiesByType = await _context.MiningActivities
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new ActivityTypeViewModel
                    {
                        ActivityType = g.Key,
                        Count = g.Count(),
                        TotalQuantity = g.Sum(a => a.Quantity)
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
        