using Microsoft.AspNetCore.Mvc;
using KogiExportHub.Models;
using System.Collections.Generic;
using System.Linq;
using KogiExportHub.Data; // Added for ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Added for Include and ToListAsync
using Microsoft.AspNetCore.Identity; // Added for UserManager
using System.Threading.Tasks; // Added for async operations
using Microsoft.AspNetCore.Authorization; // Added for Authorize attribute
using System.IO; // Added for Path and FileStream
using Microsoft.AspNetCore.Hosting; // Added for IWebHostEnvironment

namespace KogiExportHub.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context; // Added
        private readonly UserManager<IdentityUser> _userManager; // Added
        private readonly IWebHostEnvironment _webHostEnvironment; // Added

        // Updated constructor for dependency injection
        public MarketplaceController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }
        
        public async Task<IActionResult> ProductList(int? categoryId, string sortOrder, string searchString)
        {
            // Fetch products from the database
            var productsQuery = _context.Products
                                        .Include(p => p.Category) // Include category information
                                        .Include(p => p.Seller) // Include seller information
                                        .AsQueryable();
            
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString) || 
                                                 p.Description.Contains(searchString));
            }

            // Add sorting options
            ViewBag.PriceSortParm = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.DateSortParm = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            switch (sortOrder)
            {
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "date":
                    productsQuery = productsQuery.OrderBy(p => p.CreatedAt);
                    break;
                case "date_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                case "name_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Name);
                    break;
                default: // Name ascending
                    productsQuery = productsQuery.OrderBy(p => p.Name);
                    break;
            }

            var products = await productsQuery.ToListAsync();
            var categories = await _context.ProductCategories.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.CurrentSort = sortOrder;
            
            return View(products);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.Seller) // Include Seller (UserProfile)
                                            .ThenInclude(s => s.User) // THEN Include the User (IdentityUser) from UserProfile
                                        .Include(p => p.Location)
                                        .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }
        
        [HttpGet]
        [Authorize] // Only authenticated users can add products
        public async Task<IActionResult> AddProduct()
        {
            // Explicitly load categories and locations
            var categories = await _context.ProductCategories.ToListAsync();
            var locations = await _context.Locations.OrderBy(l => l.Name).ToListAsync();
            
            ViewBag.Categories = categories;
            ViewBag.Locations = locations;
            
            // Initialize a new ProductViewModel
            var model = new ProductViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            var categories = await _context.ProductCategories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.Locations = await _context.Locations.ToListAsync();

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (userProfile == null)
                {
                    ModelState.AddModelError("", "User profile not found. Please complete your profile.");
                    return View(model);
                }

                string uniqueFileName = null;
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Unit = model.Unit, // Save the unit
                    CategoryId = model.CategoryId,
                    SellerId = userProfile.Id,
                    ImageUrl = uniqueFileName != null ? Path.Combine("images", "products", uniqueFileName) : null,
                    LocationId = model.LocationId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductList));
            }
            
            return View(model);
        }

        // Add a new action to get the default unit for a category
        [HttpGet]
        public async Task<IActionResult> GetCategoryUnit(int categoryId)
        {
            var category = await _context.ProductCategories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            
            return Json(new { unit = category.DefaultUnit });
        }
    }
}