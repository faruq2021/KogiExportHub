using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Data;
using KogiExportHub.Models;
using KogiExportHub.Services; // Add this using statement
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;

namespace KogiExportHub.Controllers
{
    [Authorize]
    public class InfrastructureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPaymentService _paymentService; // Add this field

        public InfrastructureController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IPaymentService paymentService) // Add IPaymentService parameter
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _paymentService = paymentService; // Initialize the field
        }
        
        // GET: Infrastructure/Dashboard
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
            
            if (userProfile == null)
            {
                return RedirectToAction("Profile", "Account");
            }

            var userProposals = await _context.InfrastructureProposals
                .Where(p => p.ProposerId == userProfile.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new UserProposalSummary
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                    EstimatedCost = p.EstimatedCost,
                    SubmissionDate = p.CreatedAt
                })
                .ToListAsync();

            var userFundingRequests = await _context.FundingRequests
                .Where(f => f.RequesterId == userProfile.Id)
                .Include(f => f.Proposal)
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .Select(f => new UserFundingRequestSummary
                {
                    Id = f.Id,
                    ProposalTitle = f.Proposal.Title,
                    AmountRequested = f.AmountRequested,
                    Status = f.Status,
                    SubmissionDate = f.CreatedAt
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                UserProposals = userProposals,
                UserFundingRequests = userFundingRequests,
                TotalProposals = await _context.InfrastructureProposals.CountAsync(),
                ActiveProposals = await _context.InfrastructureProposals.CountAsync(p => p.Status == "Active"),
                TotalFundingRequested = await _context.FundingRequests.SumAsync(f => (decimal?)f.AmountRequested) ?? 0,
                TotalFundingApproved = await _context.FundingRequests
                    .Where(f => f.Status == "Approved")
                    .SumAsync(f => (decimal?)f.AmountRequested) ?? 0
            };

            return View(viewModel);
        }

        // GET: Infrastructure/Proposals
        public async Task<IActionResult> Proposals(string status = null)
        {
            var proposals = _context.InfrastructureProposals
                .Include(p => p.Location)
                .Include(p => p.Proposer)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                proposals = proposals.Where(p => p.Status == status);
            }

            return View(await proposals.ToListAsync());
        }

        // GET: Infrastructure/ProposalDetails/5
        public async Task<IActionResult> ProposalDetails(int id)
        {
            var proposal = await _context.InfrastructureProposals
                .Include(p => p.Location)
                .Include(p => p.Proposer)
                .Include(p => p.Category)
                .Include(p => p.FundingRequests)
                    .ThenInclude(f => f.Contributions)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null)
            {
                return NotFound();
            }

            return View(proposal);
        }

        // GET: Infrastructure/CreateProposal
        [Authorize] // Changed from [Authorize(Roles = "Admin,Seller")] to allow all authenticated users
        public async Task<IActionResult> CreateProposal()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CreateProposal", "Infrastructure") });
            }
            
            // Removed role check - now all authenticated users can create proposals
            
            // Check if user has a complete profile
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
            
            if (userProfile == null || !IsProfileComplete(userProfile))
            {
                TempData["ErrorMessage"] = "Please complete your profile before creating proposals.";
                return RedirectToAction("Profile", "Account");
            }
            
            ViewBag.Locations = await _context.Locations.ToListAsync();
            ViewBag.Categories = await _context.InfrastructureCategories.ToListAsync();
            return View();
        }

        // POST: Infrastructure/CreateProposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Changed from [Authorize(Roles = "Admin,Seller")] to allow all authenticated users
        public async Task<IActionResult> CreateProposal(InfrastructureProposal proposal, Microsoft.AspNetCore.Http.IFormFile documentation)
        {
            // Remove validation errors for navigation properties and system fields
            ModelState.Remove("Location");
            ModelState.Remove("Proposer");
            ModelState.Remove("Category");
            ModelState.Remove("FundingRequests");
            ModelState.Remove("Milestones");
            ModelState.Remove("DocumentationUrl");
            ModelState.Remove("Status");
            ModelState.Remove("ProposerId");
            
            // For draft submissions, make some fields optional
            if (string.IsNullOrEmpty(proposal.Description))
            {
                ModelState.Remove("Description");
            }
            
            if (proposal.ExpectedBeneficiaries == 0)
            {
                ModelState.Remove("ExpectedBeneficiaries");
            }
            
            if (proposal.ExpectedTimelineMonths == 0)
            {
                ModelState.Remove("ExpectedTimelineMonths");
            }
            
            if (string.IsNullOrEmpty(proposal.ExpectedImpact))
            {
                ModelState.Remove("ExpectedImpact");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (userProfile == null)
                {
                    return RedirectToAction("Profile", "Account");
                }

                proposal.ProposerId = userProfile.Id;
                proposal.Status = "Draft";
                proposal.CreatedAt = DateTime.Now;
                proposal.UpdatedAt = DateTime.Now;

                if (documentation != null && documentation.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "proposals");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + documentation.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await documentation.CopyToAsync(fileStream);
                    }

                    proposal.DocumentationUrl = "/uploads/proposals/" + uniqueFileName;
                }

                _context.Add(proposal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProposalDetails), new { id = proposal.Id });
            }

            ViewBag.Locations = await _context.Locations.ToListAsync();
            ViewBag.Categories = await _context.InfrastructureCategories.ToListAsync();
            return View(proposal);
        }

        // GET: Infrastructure/EditProposal/5
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> EditProposal(int id)
        {
            var proposal = await _context.InfrastructureProposals.FindAsync(id);
            if (proposal == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null || (proposal.ProposerId != userProfile.Id && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            ViewBag.Locations = await _context.Locations.ToListAsync();
            ViewBag.Categories = await _context.InfrastructureCategories.ToListAsync();
            return View(proposal);
        }

        // POST: Infrastructure/EditProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> EditProposal(int id, InfrastructureProposal proposal, Microsoft.AspNetCore.Http.IFormFile documentation)
        {
            if (id != proposal.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null || (proposal.ProposerId != userProfile.Id && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProposal = await _context.InfrastructureProposals.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    proposal.CreatedAt = existingProposal.CreatedAt;
                    proposal.UpdatedAt = DateTime.Now;
                    proposal.DocumentationUrl = existingProposal.DocumentationUrl; // Keep existing documentation URL unless new file is uploaded

                    if (documentation != null && documentation.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "proposals");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + documentation.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await documentation.CopyToAsync(fileStream);
                        }

                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(existingProposal.DocumentationUrl))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProposal.DocumentationUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        proposal.DocumentationUrl = "/uploads/proposals/" + uniqueFileName;
                    }

                    _context.Update(proposal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProposalExists(proposal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ProposalDetails), new { id = proposal.Id });
            }

            ViewBag.Locations = await _context.Locations.ToListAsync();
            ViewBag.Categories = await _context.InfrastructureCategories.ToListAsync();
            return View(proposal);
        }

        // POST: Infrastructure/SubmitProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> SubmitProposal(int id)
        {
            var proposal = await _context.InfrastructureProposals.FindAsync(id);
            if (proposal == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null || (proposal.ProposerId != userProfile.Id && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            proposal.Status = "Submitted";
            proposal.SubmissionDate = DateTime.Now;
            proposal.UpdatedAt = DateTime.Now;

            _context.Update(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ProposalDetails), new { id = proposal.Id });
        }

        // GET: Infrastructure/CreateFundingRequest/5
        [Authorize] // Changed from [Authorize(Roles = "Admin,Seller")] to allow all authenticated users
        public async Task<IActionResult> CreateFundingRequest(int proposalId)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CreateFundingRequest", "Infrastructure", new { proposalId }) });
            }
            
            // Removed role check - now all authenticated users can request funding
            
            var proposal = await _context.InfrastructureProposals
                .Include(p => p.Location)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == proposalId);
        
            if (proposal == null)
            {
                TempData["ErrorMessage"] = "Proposal not found.";
                return RedirectToAction("Proposals");
            }
            
            // Check if proposal is approved
            if (proposal.Status != "Approved")
            {
                TempData["ErrorMessage"] = "You can only request funding for approved proposals.";
                return RedirectToAction("ProposalDetails", new { id = proposalId });
            }
            
            // Check if user has a complete profile
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
            
            if (userProfile == null || !IsProfileComplete(userProfile))
            {
                TempData["ErrorMessage"] = "Please complete your profile before requesting funding.";
                return RedirectToAction("Profile", "Account");
            }
        
            ViewBag.Proposal = proposal;
            return View(new FundingRequest { ProposalId = proposalId });
        }

        // POST: Infrastructure/CreateFundingRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateFundingRequest(FundingRequest fundingRequest)
        {
            // Remove validation errors for fields that shouldn't be required during creation
            ModelState.Remove("Status");
            ModelState.Remove("RejectionReason");
            ModelState.Remove("ProfitSharingTerms");
            ModelState.Remove("Proposal");
            ModelState.Remove("Requester");
            ModelState.Remove("Contributions");
            
            // Only require ProfitSharingTerms if Sharia compliant
            if (fundingRequest.IsShariaCompliant && string.IsNullOrEmpty(fundingRequest.ProfitSharingTerms))
            {
                ModelState.AddModelError("ProfitSharingTerms", "Profit sharing terms are required for Sharia-compliant funding.");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
        
                if (userProfile == null)
                {
                    return RedirectToAction("Profile", "Account");
                }
        
                // Set the required fields
                fundingRequest.RequesterId = userProfile.Id;
                fundingRequest.Status = "Pending";
                fundingRequest.SubmissionDate = DateTime.Now;
                fundingRequest.CreatedAt = DateTime.Now;
                fundingRequest.UpdatedAt = DateTime.Now;
                
                // Handle database constraints - set default values instead of NULL
                if (!fundingRequest.IsShariaCompliant || string.IsNullOrEmpty(fundingRequest.ProfitSharingTerms))
                {
                    fundingRequest.ProfitSharingTerms = "N/A"; // Database doesn't allow NULL
                }
                
                if (string.IsNullOrEmpty(fundingRequest.RejectionReason))
                {
                    fundingRequest.RejectionReason = "N/A"; // Database doesn't allow NULL
                }
        
                _context.Add(fundingRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProposalDetails), new { id = fundingRequest.ProposalId });
            }
        
            // If validation fails, return to view with errors
            var proposal = await _context.InfrastructureProposals
                .Include(p => p.Location)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == fundingRequest.ProposalId);
            
            ViewBag.Proposal = proposal;
            return View(fundingRequest);
        }

        // GET: Infrastructure/FundingRequestDetails/5
        public async Task<IActionResult> FundingRequestDetails(int id)
        {
            var fundingRequest = await _context.FundingRequests
                .Include(f => f.Proposal)
                    .ThenInclude(p => p.Category)
                .Include(f => f.Proposal)
                    .ThenInclude(p => p.Location)
                .Include(f => f.Requester)
                .Include(f => f.Contributions)
                    .ThenInclude(c => c.Contributor)  // Add this line
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fundingRequest == null)
            {
                return NotFound();
            }

            return View(fundingRequest);
        }

        // GET: Infrastructure/FundingRequests
        public async Task<IActionResult> FundingRequests()
        {
            var fundingRequests = await _context.FundingRequests
                .Include(f => f.Proposal)
                .Include(f => f.Requester)
                .OrderByDescending(f => f.Id)
                .ToListAsync();
            
            return View(fundingRequests);
        }

        // POST: Infrastructure/ContributeToFunding
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ContributeToFunding(int fundingRequestId, decimal amount)
        {
            var fundingRequest = await _context.FundingRequests.FindAsync(fundingRequestId);
            if (fundingRequest == null)
            {
                return NotFound();
            }
        
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
        
            if (userProfile == null)
            {
                return RedirectToAction("Profile", "Account");
            }
        
            // Generate a temporary transaction reference
            var tempTransactionRef = $"TEMP_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        
            // Here you would integrate with your payment service
            // For now, we'll just create a pending contribution
            var contribution = new FundingContribution
            {
                FundingRequestId = fundingRequestId,
                ContributorId = userProfile.Id,
                Amount = amount,
                TransactionReference = tempTransactionRef, // Add this line
                Status = "Pending",
                ContributionDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        
            _context.Add(contribution);
            await _context.SaveChangesAsync();
        
            // Redirect to payment processing
            return RedirectToAction("InitializePayment", "Payment", new { contributionId = contribution.Id });
        }

        // Helper method to check if a proposal exists
        private bool ProposalExists(int id)
        {
            return _context.InfrastructureProposals.Any(e => e.Id == id);
        }

        // Helper method to check profile completion
        private bool IsProfileComplete(UserProfile profile)
        {
            return !string.IsNullOrEmpty(profile.FirstName) &&
                   !string.IsNullOrEmpty(profile.LastName) &&
                   !string.IsNullOrEmpty(profile.PhoneNumber) &&
                   !string.IsNullOrEmpty(profile.Email) &&
                   !string.IsNullOrEmpty(profile.Role);
        }

        // POST: Infrastructure/ApproveProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveProposal(int id)
        {
            var proposal = await _context.InfrastructureProposals.FindAsync(id);
            if (proposal == null)
            {
                return NotFound();
            }

            proposal.Status = "Approved";
            proposal.ApprovalDate = DateTime.Now;
            proposal.UpdatedAt = DateTime.Now;

            _context.Update(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ProposalDetails), new { id = proposal.Id });
        }

        // POST: Infrastructure/RejectProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectProposal(int id, string rejectionReason)
        {
            var proposal = await _context.InfrastructureProposals.FindAsync(id);
            if (proposal == null)
            {
                return NotFound();
            }

            proposal.Status = "Rejected";
            proposal.UpdatedAt = DateTime.Now;
            // You might want to add a RejectionReason field to the model

            _context.Update(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ProposalDetails), new { id = proposal.Id });
        }

        // GET: Infrastructure/AdminReview
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReview()
        {
            var submittedProposals = await _context.InfrastructureProposals
                .Include(p => p.Proposer)
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Where(p => p.Status == "Submitted")
                .OrderBy(p => p.SubmissionDate)
                .ToListAsync();

            return View(submittedProposals);
        }

        // GET: Infrastructure/AdminFundingReview
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminFundingReview()
        {
            var pendingFundingRequests = await _context.FundingRequests
                .Include(f => f.Proposal)
                    .ThenInclude(p => p.Category)
                .Include(f => f.Proposal)
                    .ThenInclude(p => p.Location)
                .Include(f => f.Requester)
                .Include(f => f.Contributions)
                .Where(f => f.Status == "Pending")
                .OrderBy(f => f.CreatedAt)
                .ToListAsync();

            return View(pendingFundingRequests);
        }

        // POST: Infrastructure/ApproveFunding
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveFunding(int id, decimal? amountApproved, string adminComments, 
            string recipientAccount, string bankCode)
        {
            // In the ApproveFunding method, around line 640-650, modify the funding request query to include the Requester:
            var fundingRequest = await _context.FundingRequests
                .Include(fr => fr.Requester) // Add this line to load the Requester navigation property
                .FirstOrDefaultAsync(fr => fr.Id == id);
            
            if (fundingRequest == null)
            {
                return NotFound();
            }
            
            if (fundingRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This funding request has already been processed.";
                return RedirectToAction(nameof(AdminFundingReview));
            }

            // Validate approved amount
            if (amountApproved.HasValue && amountApproved.Value > fundingRequest.AmountRequested)
            {
                TempData["ErrorMessage"] = "Approved amount cannot exceed requested amount.";
                return RedirectToAction(nameof(AdminFundingReview));
            }
            
            // Update funding request
            fundingRequest.Status = "Approved";
            fundingRequest.AmountApproved = amountApproved;
            fundingRequest.AdminComments = adminComments;
            fundingRequest.ApprovalDate = DateTime.Now;
            fundingRequest.RecipientAccountNumber = recipientAccount;
            fundingRequest.RecipientBankCode = bankCode;
            fundingRequest.RecipientAccountName = recipientAccount;
            
            await _context.SaveChangesAsync();
            
            // Also, add a null check before accessing Requester.AccountName:
            var recipientName = fundingRequest.Requester?.AccountName ?? "Unknown";
            
            // Initiate disbursement
            var transferResult = await _paymentService.InitiateTransferAsync(
                amountApproved.Value,
                recipientAccount, 
                bankCode, 
                recipientName, // Use the null-safe variable
                $"Infrastructure funding for proposal {fundingRequest.ProposalId}"
            );
            
            if (transferResult.Status == "success")
            {
                fundingRequest.DisbursementTransferId = transferResult.TransferId;
                fundingRequest.DisbursementReference = transferResult.Reference;
                fundingRequest.DisbursementStatus = "Pending";
                fundingRequest.Status = "Funded";
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("AdminFundingReview");
        }

        // POST: Infrastructure/RejectFunding
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectFunding(int id, string rejectionReason)
        {
            var fundingRequest = await _context.FundingRequests
                .Include(f => f.Proposal)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fundingRequest == null)
            {
                return NotFound();
            }

            if (fundingRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This funding request has already been processed.";
                return RedirectToAction(nameof(AdminFundingReview));
            }

            fundingRequest.Status = "Rejected";
            fundingRequest.AdminComments = rejectionReason;
            fundingRequest.ApprovalDate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Funding request for {fundingRequest.Proposal.Title} has been rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting the funding request.";
            }

            return RedirectToAction(nameof(AdminFundingReview));
        }
    }
}