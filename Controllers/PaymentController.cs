using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KogiExportHub.Services;
using KogiExportHub.Models;
using Microsoft.AspNetCore.Identity;
using KogiExportHub.Data;
using Microsoft.EntityFrameworkCore;

namespace KogiExportHub.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IReceiptService _receiptService;
        private readonly EmailService _emailService;

        // Add ITaxService to constructor dependencies
        private readonly ITaxService _taxService;

        public PaymentController(
            IPaymentService paymentService, 
            ICartService cartService, 
            UserManager<IdentityUser> userManager, 
            ApplicationDbContext context,
            IReceiptService receiptService,
            EmailService emailService,
            ITaxService taxService) // Add this parameter
        {
            _paymentService = paymentService;
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
            _receiptService = receiptService;
            _emailService = emailService;
            _taxService = taxService; // Add this line
        }

        [HttpPost]
        public async Task<IActionResult> InitializePayment()
        {
            try
            {
                var cart = _cartService.GetCart(HttpContext);
                if (!cart.Items.Any())
                {
                    return BadRequest(new { error = "Cart is empty" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                var result = await _paymentService.InitializePaymentAsync(
                    cart.Total,
                    "NGN",
                    userProfile?.Email ?? user.Email ?? "",
                    $"{userProfile?.FirstName} {userProfile?.LastName}"
                );

                if (result.Status == "success")
                {
                    return Json(new { 
                        success = true, 
                        paymentLink = result.PaymentLink,
                        transactionId = result.TransactionId
                    });
                }

                return BadRequest(new { error = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> InitializePayment(int contributionId)
        {
            try
            {
                var contribution = await _context.FundingContributions
                    .Include(c => c.FundingRequest)
                    .ThenInclude(fr => fr.Proposal)
                    .Include(c => c.Contributor)
                    .FirstOrDefaultAsync(c => c.Id == contributionId);

                if (contribution == null)
                {
                    return NotFound("Contribution not found");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (userProfile?.Id != contribution.ContributorId)
                {
                    return Forbid("You can only pay for your own contributions");
                }

                var result = await _paymentService.InitializePaymentAsync(
                    contribution.Amount,
                    "NGN",
                    userProfile.Email ?? user.Email ?? "",
                    $"{userProfile.FirstName} {userProfile.LastName}"
                );

                if (result.Status == "success")
                {
                    // Update contribution with transaction reference
                    contribution.TransactionReference = result.TransactionId;
                    contribution.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Redirect to payment link
                    return Redirect(result.PaymentLink);
                }

                TempData["ErrorMessage"] = $"Payment initialization failed: {result.Message}";
                return RedirectToAction("ProposalDetails", "Infrastructure", new { id = contribution.FundingRequest.ProposalId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Infrastructure");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string status, string tx_ref, string transaction_id)
        {
            if (status == "successful")
            {
                var verification = await _paymentService.VerifyPaymentAsync(transaction_id);
                
                if (verification.IsSuccessful)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var userProfile = await _context.UserProfiles
                        .FirstOrDefaultAsync(up => up.UserId == user.Id);

                    if (userProfile != null)
                    {
                        // Check if this is a funding contribution using tx_ref instead of transaction_id
                        var contribution = await _context.FundingContributions
                            .Include(c => c.FundingRequest)
                            .FirstOrDefaultAsync(c => c.TransactionReference == tx_ref);

                        if (contribution != null)
                        {
                            // Handle funding contribution
                            contribution.Status = "Completed";
                            contribution.UpdatedAt = DateTime.Now;
                            await _context.SaveChangesAsync();

                            TempData["SuccessMessage"] = "Thank you for your contribution! Your support helps build Kogi State's infrastructure.";
                            TempData["ContributionAmount"] = contribution.Amount.ToString("N2"); // Convert decimal to string
                            TempData["FundingRequestId"] = contribution.FundingRequestId;
                            return RedirectToAction("ContributionSuccess");
                        }
                        else
                        {
                            // Handle marketplace order (existing logic)
                            var cart = _cartService.GetCart(HttpContext);
                            
                            foreach (var item in cart.Items)
                            {
                                var product = await _context.Products
                                    .Include(p => p.Seller)
                                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                                if (product != null)
                                {
                                    var newTransaction = new Transaction
                                    {
                                        ProductId = product.Id,
                                        BuyerId = userProfile.Id,
                                        Quantity = item.Quantity,
                                        TotalAmount = item.Quantity * product.Price,
                                        Status = "Completed",
                                        PaymentMethod = "Flutterwave",
                                        TransactionReference = transaction_id,
                                        CreatedAt = DateTime.Now,
                                        UpdatedAt = DateTime.Now
                                    };

                                    _context.Transactions.Add(newTransaction);
                                    await _context.SaveChangesAsync();

                                    // Process taxes for the transaction
                                    await _taxService.ProcessTaxesForTransactionAsync(newTransaction);

                                    // Update product stock if applicable
                                    if (product.Quantity > 0)
                                    {
                                        product.Quantity -= item.Quantity;
                                    }
                                }
                            }

                            await _context.SaveChangesAsync();
                            
                            // Clear the cart
                            _cartService.ClearCart(HttpContext);
                            
                            TempData["SuccessMessage"] = "Payment successful! Your order has been processed.";
                            return RedirectToAction("Success");
                        }
                    }
                }
            }

            TempData["ErrorMessage"] = "Payment failed or was cancelled.";
            return RedirectToAction("Failed");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Failed()
        {
            return View();
        }

        public IActionResult ContributionSuccess()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int transactionId)
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
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.BuyerId == userProfile.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            var receiptHtml = await _receiptService.GenerateReceiptHtmlAsync(transaction);
            var receiptBytes = System.Text.Encoding.UTF8.GetBytes(receiptHtml);

            return File(receiptBytes, "text/html", $"Receipt_{transaction.Id}.html");
        }
    }

    public class CreatePaymentIntentRequest
    {
        public decimal Amount { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}