using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using KogiExportHub.Infrastructure;
using System;

namespace KogiExportHub.Controllers
{
    [RoleAuthorization("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main Admin Dashboard
        // Add this Dashboard action method after the Index method
        public async Task<IActionResult> Dashboard()
        {
            var pendingInvestors = await _context.Investors
                .Where(i => i.Status == "Pending")
                .ToListAsync();
                
            var recentInvestors = await _context.Investors
                .OrderByDescending(i => i.RegistrationDate)
                .Take(10)
                .ToListAsync();
                
            var viewModel = new AdminDashboardViewModel
            {
                // Get pending items for approval
                PendingMiners = await _context.Miners
                    .Where(m => m.LicenseStatus == "Pending")
                    .Include(m => m.Location)
                    .ToListAsync(),
                    
                PendingInvestors = pendingInvestors,
                
                // Add the missing properties
                PendingInvestorRegistrations = pendingInvestors,
                RecentInvestorActivity = recentInvestors,
                    
                PendingInfrastructure = await _context.InfrastructureProposals
                    .Where(p => p.Status == "Pending Approval")
                    .ToListAsync(),
                    
                PendingTaxReturns = await _context.TaxCalculations
                    .Where(t => t.Status == "Pending Review")
                    .ToListAsync(),
                    
                RecentActivities = await GetRecentActivities(),
                
                // Add statistics
                TotalUsers = await _context.UserProfiles.CountAsync(),
                TotalInvestors = await _context.Investors.CountAsync(),
                VerifiedInvestors = await _context.Investors.Where(i => i.VerificationStatus == "Verified").CountAsync(),
                RejectedInvestors = await _context.Investors.Where(i => i.VerificationStatus == "Rejected").CountAsync(),
                
                // Add recent registrations
                RecentRegistrations = await _context.UserProfiles
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // Add missing admin action methods
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.UserProfiles
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
                
            return View(users);
        }

        public async Task<IActionResult> InvestorManagement(string status = "All")
        {
            var query = _context.Investors.AsQueryable();
            
            if (status != "All")
            {
                query = query.Where(i => i.VerificationStatus == status);
            }
            
            var investors = await query.OrderByDescending(i => i.RegistrationDate).ToListAsync();
            ViewBag.CurrentStatus = status;
            
            return View(investors);
        }

        public async Task<IActionResult> ReviewInvestor(int? id)
        {
            if (id.HasValue)
            {
                // Single investor review
                var investor = await _context.Investors.FindAsync(id.Value);
                if (investor == null)
                {
                    return NotFound();
                }
                return View(investor);
            }
            else
            {
                // List of pending investors
                var pendingInvestors = await _context.Investors
                    .Where(i => i.VerificationStatus == "Pending")
                    .OrderBy(i => i.RegistrationDate)
                    .ToListAsync();
                    
                return View(pendingInvestors);
            }
        }

        public async Task<IActionResult> InfrastructureReview()
        {
            var pendingProjects = await _context.InfrastructureProposals
                .Where(p => p.Status == "Pending Approval")
                .Include(p => p.Location)
                .Include(p => p.Category)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();
                
            return View(pendingProjects);
        }

        public async Task<IActionResult> FundingReview()
        {
            var pendingFunding = await _context.FundingRequests
                .Where(f => f.Status == "Pending")
                .Include(f => f.Proposal)
                .ThenInclude(p => p.Location)
                .Include(f => f.Proposal)
                .ThenInclude(p => p.Category)
                .OrderBy(f => f.CreatedAt)
                .ToListAsync();
                
            return View(pendingFunding);
        }

        // Miner Approval Management
        public async Task<IActionResult> MinerApprovals()
        {
            var pendingMiners = await _context.Miners
                .Where(m => m.LicenseStatus == "Pending")
                .Include(m => m.Location)
                .OrderBy(m => m.RegistrationDate)
                .ToListAsync();
                
            return View(pendingMiners);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveMiner(int id, string decision, string notes = "")
        {
            var miner = await _context.Miners.FindAsync(id);
            if (miner == null)
            {
                return NotFound();
            }

            if (decision == "approve")
            {
                miner.LicenseStatus = "Active";
                miner.IssueDate = DateTime.Now;
                miner.ExpiryDate = DateTime.Now.AddYears(2); // 2-year license
                
                // Log approval activity
                await LogAdminActivity("Miner Approved", $"Approved miner registration for {miner.FullName}");
            }
            else if (decision == "reject")
            {
                miner.LicenseStatus = "Rejected";
                miner.Notes = notes;
                
                await LogAdminActivity("Miner Rejected", $"Rejected miner registration for {miner.FullName}. Reason: {notes}");
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Miner registration {decision}d successfully!";
            return RedirectToAction(nameof(MinerApprovals));
        }

        // Investor Approval Management
        public async Task<IActionResult> InvestorApprovals()
        {
            var pendingInvestors = await _context.Investors
                .Where(i => i.Status == "Pending")
                .OrderBy(i => i.RegistrationDate)
                .ToListAsync();
                
            return View(pendingInvestors);
        }

        // Replace the existing ApproveInvestor method (around line 150) with this:
        [HttpPost]
        public async Task<IActionResult> ApproveInvestorDecision(int id, string decision, string notes = "")
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }

            if (decision == "approve")
            {
                investor.VerificationStatus = "Verified";
                investor.VerificationDate = DateTime.Now;
                investor.AdminComments = notes;
                
                await LogAdminActivity("Investor Approved", $"Approved investor registration for {investor.CompanyName}");
            }
            else if (decision == "reject")
            {
                investor.VerificationStatus = "Rejected";
                investor.VerificationDate = DateTime.Now;
                investor.RejectionReason = notes;
                
                await LogAdminActivity("Investor Rejected", $"Rejected investor registration for {investor.CompanyName}. Reason: {notes}");
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Investor registration {decision}d successfully!";
            return RedirectToAction(nameof(InvestorApprovals));
        }

        // Infrastructure Approval Management
        public async Task<IActionResult> InfrastructureApprovals()
        {
            var pendingProjects = await _context.InfrastructureProposals
                .Where(p => p.Status == "Pending Approval")
                .Include(p => p.Location)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();
                
            return View(pendingProjects);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveInfrastructure(int id, string decision, string notes = "")
        {
            var project = await _context.InfrastructureProposals.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            if (decision == "approve")
            {
                project.Status = "Approved";
                project.ApprovalDate = DateTime.Now;
                
                await LogAdminActivity("Infrastructure Approved", $"Approved infrastructure project: {project.Title}");
            }
            else if (decision == "reject")
            {
                project.Status = "Rejected";
                project.Notes = notes;
                
                await LogAdminActivity("Infrastructure Rejected", $"Rejected infrastructure project: {project.Title}. Reason: {notes}");
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Infrastructure project {decision}d successfully!";
            return RedirectToAction(nameof(InfrastructureApprovals));
        }

        // Tax Return Review Management
        public async Task<IActionResult> TaxReturns()
        {
            var pendingReturns = await _context.TaxCalculations
                .Where(t => t.Status == "Pending Review")
                .Include(t => t.Transaction)
                .OrderBy(t => t.CalculationDate)
                .ToListAsync();
                
            return View(pendingReturns);
        }

        [HttpPost]
        public async Task<IActionResult> ReviewTaxReturn(int id, string decision, string notes = "")
        {
            var taxCalculation = await _context.TaxCalculations
                .Include(t => t.Transaction)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (taxCalculation == null)
            {
                return NotFound();
            }

            if (decision == "approve")
            {
                taxCalculation.Status = "Approved";
                
                await LogAdminActivity("Tax Calculation Approved", $"Approved tax calculation for transaction {taxCalculation.TransactionId}");
            }
            else if (decision == "reject")
            {
                taxCalculation.Status = "Rejected";
                
                await LogAdminActivity("Tax Calculation Rejected", $"Rejected tax calculation for transaction {taxCalculation.TransactionId}. Reason: {notes}");
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Tax calculation {decision}d successfully!";
            return RedirectToAction(nameof(TaxReturns));
        }

        // System Settings
        public async Task<IActionResult> Settings()
        {
            // Load system settings
            return View();
        }

        // Activity Logs
        public async Task<IActionResult> ActivityLogs()
        {
            var activities = await _context.AdminActivities
                .OrderByDescending(a => a.CreatedDate)
                .Take(100)
                .ToListAsync();
                
            return View(activities);
        }

        // Helper Methods
        private async Task LogAdminActivity(string action, string description)
        {
            var activity = new AdminActivity
            {
                Action = action,
                Description = description,
                AdminUser = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            _context.AdminActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        private async Task<List<AdminActivity>> GetRecentActivities()
        {
            return await _context.AdminActivities
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();
        }

        // Add these methods before the closing brace
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInvestorAction(int id, string adminComments = "")
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }

            investor.VerificationStatus = "Verified";
            investor.VerificationDate = DateTime.Now;
            investor.AdminComments = adminComments;
            
            await _context.SaveChangesAsync();
            
            await LogAdminActivity("Investor Approved", $"Approved investor {investor.CompanyName}");
            
            TempData["SuccessMessage"] = $"Investor {investor.CompanyName} has been approved successfully.";
            return RedirectToAction("InvestorManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInvestorAction(int id, string rejectionReason)
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }

            investor.VerificationStatus = "Rejected";
            investor.VerificationDate = DateTime.Now;
            investor.RejectionReason = rejectionReason;
            
            await _context.SaveChangesAsync();
            
            await LogAdminActivity("Investor Rejected", $"Rejected investor {investor.CompanyName}. Reason: {rejectionReason}");
            
            TempData["SuccessMessage"] = $"Investor {investor.CompanyName} has been rejected.";
            return RedirectToAction("InvestorManagement");
        }

        [HttpGet]
        public async Task<IActionResult> ReviewInvestor(int id)
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }
            return View(investor);
        }

        // User Management Actions
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserProfile userProfile)
        {
            if (id != userProfile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userProfile);
                    await _context.SaveChangesAsync();
                    
                    await LogAdminActivity("User Updated", $"Updated user profile for {userProfile.FirstName} {userProfile.LastName}");
                    
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(UserManagement));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserProfileExists(userProfile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(userProfile);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id, UserProfile userProfile)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user != null)
            {
                _context.UserProfiles.Remove(user);
                await _context.SaveChangesAsync();
                
                await LogAdminActivity("User Deleted", $"Deleted user profile for {user.FirstName} {user.LastName}");
                
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            return RedirectToAction(nameof(UserManagement));
        }

        private async Task<bool> UserProfileExists(int id)
        {
            return await _context.UserProfiles.AnyAsync(e => e.Id == id);
        }
    }
}
