using System;
using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public int SellerId { get; set; } // Foreign key to UserProfile
        public int CategoryId { get; set; } // Foreign key to ProductCategory
        public int? LocationId { get; set; } // Foreign key to Location
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual UserProfile Seller { get; set; }
        public virtual ProductCategory Category { get; set; }
        public virtual Location Location { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        
        // Add this property to your Product class
        public string Unit { get; set; } // Unit of measurement
    }
}