using Microsoft.AspNetCore.Mvc;
using KogiExportHub.Models;
using KogiExportHub.Services;
using KogiExportHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace KogiExportHub.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public CartController(ICartService cartService, ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _cartService = cartService;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart(HttpContext);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (quantity > product.Quantity)
            {
                return Json(new { success = false, message = $"Only {product.Quantity} {product.Unit}(s) available" });
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                Unit = product.Unit,
                ImageUrl = product.ImageUrl ?? string.Empty,
                SellerName = $"{product.Seller?.FirstName} {product.Seller?.LastName}".Trim(),
                SellerId = product.SellerId,
                AvailableQuantity = product.Quantity
            };

            _cartService.AddToCart(HttpContext, cartItem);
            var cart = _cartService.GetCart(HttpContext);

            return Json(new { 
                success = true, 
                message = "Product added to cart successfully!",
                cartCount = cart.TotalItems,
                cartTotal = cart.Total.ToString("C")
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(HttpContext, productId);
            var cart = _cartService.GetCart(HttpContext);
            
            return Json(new { 
                success = true,
                cartCount = cart.TotalItems,
                cartTotal = cart.Total.ToString("C")
            });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateCartItemQuantity(HttpContext, productId, quantity);
            var cart = _cartService.GetCart(HttpContext);
            
            return Json(new { 
                success = true,
                cartCount = cart.TotalItems,
                cartTotal = cart.Total.ToString("C")
            });
        }

        public IActionResult GetCartCount()
        {
            var cart = _cartService.GetCart(HttpContext);
            return Json(new { count = cart.TotalItems });
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = _cartService.GetCart(HttpContext);
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index");
            }
        
            var model = new CheckoutViewModel
            {
                Cart = cart
            };
        
            // Pass Stripe publishable key to view
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
        
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cart = _cartService.GetCart(HttpContext);
                return View("Checkout", model);
            }

            var cart = _cartService.GetCart(HttpContext);
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Index");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userProfile == null)
            {
                TempData["Error"] = "Please complete your profile before making a purchase";
                return RedirectToAction("Profile", "Account");
            }

            // Create transactions for each cart item
            foreach (var item in cart.Items)
            {
                var transaction = new Transaction
                {
                    ProductId = item.ProductId,
                    BuyerId = userProfile.Id,
                    Quantity = item.Quantity,
                    TotalAmount = item.TotalPrice,
                    Status = "Pending",
                    PaymentMethod = model.PaymentMethod,
                    TransactionReference = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
            _cartService.ClearCart(HttpContext);

            TempData["Success"] = "Order placed successfully! You will be contacted for payment and delivery details.";
            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}