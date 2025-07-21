using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class ProductViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price per Unit")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public string Unit { get; set; } // Unit of measurement

        public IFormFile ImageFile { get; set; } // For file upload

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public int? LocationId { get; set; } // Optional location

        // For the calculator
        public decimal TotalPrice { get; set; }
    }
}