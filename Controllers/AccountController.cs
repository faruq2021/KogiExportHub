using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using KogiExportHub.Models;
using KogiExportHub.Data;
using KogiExportHub.Services;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using KogiExportHub.Infrastructure;

namespace KogiExportHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
    
        // Update the constructor to include IEmailSender
        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
        }
    
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
    
                if (result.Succeeded)
                {
                    // Create user profile
                    var userProfile = new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Role = model.Role,
                        ProfilePictureUrl = "/images/profiles/default-profile.png", // Default image path
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
    
                    _context.UserProfiles.Add(userProfile);
                    await _context.SaveChangesAsync();
    
                    // Assign role
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);
    
                    // Generate email confirmation token
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code },
                        protocol: HttpContext.Request.Scheme);
    
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email", // Cast to specific interface implementation
                        $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
    
                    return RedirectToAction("RegisterConfirmation", new { email = model.Email });
                }
    
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
    
            // If we got this far, something failed, redisplay form
            return View(model);
        }
    
        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }
    
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
    
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }
    
            return View();
        }
    
        // GET: /Account/RegisterConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }
    
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
    
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
    
            if (userProfile == null)
            {
                return NotFound();
            }
            
            var model = new ProfileViewModel
            {
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email,
                PhoneNumber = userProfile.PhoneNumber,
                Role = userProfile.Role,
                ProfilePictureUrl = userProfile.ProfilePictureUrl
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            // Get the current user and profile
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
    
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
    
            if (userProfile == null)
            {
                return NotFound();
            }
            
            // Preserve the Role value from the database
            model.Role = userProfile.Role;
            
            // Preserve the existing ProfilePictureUrl if no new picture is uploaded
            // and clear any model state errors for ProfilePicture as it's handled separately
            if (model.ProfilePicture == null)
            {
                model.ProfilePictureUrl = userProfile.ProfilePictureUrl;
                ModelState.Remove(nameof(model.ProfilePicture)); // Clear validation error for ProfilePicture
            }
            
            if (!ModelState.IsValid)
            {
                // Repopulate ProfilePictureUrl for the view if validation fails
                // This ensures the image is still displayed correctly on the form
                if (string.IsNullOrEmpty(model.ProfilePictureUrl) && userProfile != null)
                {
                    model.ProfilePictureUrl = userProfile.ProfilePictureUrl;
                }
                return View(model);
            }
    
            // Update profile
            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.UpdatedAt = DateTime.Now;
    
            bool profilePictureUpdated = false;
            
            // Handle profile picture upload
            if (model.ProfilePicture != null)
            {
                // Create directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
    
                // Generate unique filename
                var uniqueFileName = $"{user.Id}_{Guid.NewGuid().ToString()}_{model.ProfilePicture.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
    
                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }
    
                // Update profile picture URL
                userProfile.ProfilePictureUrl = $"/images/profiles/{uniqueFileName}";
                profilePictureUpdated = true;
            }
            else
            {
                // Keep the existing profile picture URL
                userProfile.ProfilePictureUrl = model.ProfilePictureUrl;
            }
    
            _context.Update(userProfile);
            await _context.SaveChangesAsync();
    
            // Add success message based on what was updated
            if (profilePictureUpdated)
            {
                TempData["SuccessMessage"] = "Profile and profile picture updated successfully!";
            }
            else
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
    
            return RedirectToAction(nameof(Profile));
        }

        // POST: /Account/UpdateProfilePicture
        // POST: /Account/UpdateProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile ProfilePicture)
        {
            // Get the current user and profile
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
    
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
    
            if (userProfile == null)
            {
                return NotFound();
            }
            
            // Handle profile picture upload
            if (ProfilePicture != null)
            {
                // Create directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
    
                // Generate unique filename
                var uniqueFileName = $"{user.Id}_{Guid.NewGuid().ToString()}_{ProfilePicture.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
    
                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(fileStream);
                }
    
                // Update profile picture URL
                userProfile.ProfilePictureUrl = $"/images/profiles/{uniqueFileName}";
                
                _context.Update(userProfile);
                await _context.SaveChangesAsync();
            }
    
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
    
                // Generate password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { code },
                    protocol: HttpContext.Request.Scheme);
    
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");
    
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
    
            return View(model);
        }
    
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var model = new ResetPasswordViewModel { Code = code };
                return View(model);
            }
        }
    
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
    
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
    
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
    
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View();
        }
    
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    
        // GET: /Account/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
    
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
    
            if (userProfile == null)
            {
                return NotFound();
            }
    
            // Redirect to role-specific dashboard
            switch (userProfile.Role)
            {
                case "Admin":
                    return RedirectToAction("AdminDashboard");
                case "Farmer":
                    return RedirectToAction("FarmerDashboard");
                case "Miner":
                    return RedirectToAction("MinerDashboard");
                case "Aggregator":
                    return RedirectToAction("AggregatorDashboard");
                case "Investor":
                    return RedirectToAction("InvestorDashboard");
                case "Technician":
                    return RedirectToAction("TechnicianDashboard");
                case "GovernmentOfficial":
                    return RedirectToAction("GovernmentDashboard");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    
        // GET: /Account/AdminDashboard
        [HttpGet]
        [RoleAuthorization("Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var userCount = await _userManager.Users.CountAsync();
            var usersByRole = await _context.UserProfiles
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();
    
            // Investor Statistics
            var totalInvestors = await _context.Investors.CountAsync();
            var verifiedInvestors = await _context.Investors.CountAsync(i => i.VerificationStatus == "Verified");
            var pendingInvestors = await _context.Investors.CountAsync(i => i.VerificationStatus == "Pending");
            var rejectedInvestors = await _context.Investors.CountAsync(i => i.VerificationStatus == "Rejected");
            
            // Pending Investor Registrations
            var pendingRegistrations = await _context.Investors
                .Where(i => i.VerificationStatus == "Pending")
                .OrderBy(i => i.RegistrationDate)
                .ToListAsync();
            
            // Recent Investor Activity
            var recentActivity = await _context.Investors
                .OrderByDescending(i => i.RegistrationDate)
                .Take(5)
                .ToListAsync();
    
            // Around line 520-530, change the model assignment:
            var model = new AdminDashboardViewModel
            {
                TotalUsers = userCount,
                UsersByRole = usersByRole.ToDictionary(x => x.Role, x => x.Count),
                TotalInvestors = totalInvestors,
                VerifiedInvestors = verifiedInvestors,
                PendingInvestors = pendingRegistrations, // List<Investor> - this will automatically set PendingInvestorsCount
                // Remove this line: PendingInvestorsCount = pendingInvestors, // This was causing the error
                RejectedInvestors = rejectedInvestors,
                PendingInvestorRegistrations = pendingRegistrations,
                RecentInvestorActivity = recentActivity
            };
    
            return View(model);
        }
    
        // GET: /Account/FarmerDashboard
        [HttpGet]
        [RoleAuthorization("Farmer")]
        public async Task<IActionResult> FarmerDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
    
            var products = await _context.Products
                .Where(p => p.SellerId == userProfile.Id)
                .Include(p => p.Category)
                .ToListAsync();
    
            var model = new FarmerDashboardViewModel
            {
                UserProfile = userProfile,
                Products = products
            };
    
            return View(model);
        }

        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Message = "You don't have permission to access this feature. Please contact an administrator or upgrade your account.";
            return View();
        }

        public IActionResult InsufficientRole(string feature = null, string requiredRoles = null)
        {
            ViewBag.Feature = feature;
            ViewBag.RequiredRoles = requiredRoles;
            ViewBag.CurrentRole = User.IsInRole("Admin") ? "Admin" : 
                                 User.IsInRole("Seller") ? "Seller" : 
                                 User.IsInRole("Buyer") ? "Buyer" : "Guest";
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        // GET: Investor Details for Review
        [HttpGet]
        [RoleAuthorization("Admin")]
        public async Task<IActionResult> ReviewInvestor(int id)
        {
            var investor = await _context.Investors.FindAsync(id);
            if (investor == null)
            {
                return NotFound();
            }
            return View(investor);
        }

        // POST: Approve Investor
        [HttpPost]
        [RoleAuthorization("Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInvestor(int id, string adminComments = "")
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
            
            // Send approval email to investor
            // await _emailService.SendInvestorApprovalEmail(investor.Email, investor.CompanyName);
            
            TempData["SuccessMessage"] = $"Investor {investor.CompanyName} has been approved successfully.";
            return RedirectToAction("InvestorManagement");
        }

        // POST: Reject Investor
        [HttpPost]
        [RoleAuthorization("Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInvestor(int id, string rejectionReason)
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
            
            // Send rejection email to investor
            // await _emailService.SendInvestorRejectionEmail(investor.Email, investor.CompanyName, rejectionReason);
            
            TempData["SuccessMessage"] = $"Investor {investor.CompanyName} has been rejected.";
            return RedirectToAction("InvestorManagement");
        }

        // GET: All Investors Management
        [HttpGet]
        [RoleAuthorization("Admin")]
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

        // GET: /Account/SimpleResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SimpleResetPassword()
        {
            return View();
        }

        // POST: /Account/SimpleResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimpleResetPassword(SimpleResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                ModelState.AddModelError(string.Empty, "If the email exists, the password has been reset.");
                return View(model);
            }

            // Generate a password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset the password using the token
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password has been reset successfully. You can now log in with your new password.";
                return RedirectToAction("Login");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View(model);
        }
    } // This should be the ONLY closing brace for the AccountController class
} // This closes the namespace