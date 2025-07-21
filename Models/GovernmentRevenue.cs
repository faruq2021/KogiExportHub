using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class GovernmentRevenue
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime RevenueDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RevenueType { get; set; } // Tax, License, Fee, etc.
        
        [Required]
        [StringLength(100)]
        public string Source { get; set; } // Transaction, Mining License, etc.
        
        [Required]
        public decimal Amount { get; set; }
        
        [StringLength(100)]
        public string Category { get; set; } // VAT, Export Tax, etc.
        
        [StringLength(50)]
        public string Location { get; set; }
        
        public int? TransactionId { get; set; }
        public int? TaxCalculationId { get; set; }
        
        [StringLength(200)]
        public string Description { get; set; }
        
        [StringLength(100)]
        public string ReferenceNumber { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Transaction Transaction { get; set; }
        public virtual TaxCalculation TaxCalculation { get; set; }
    }
}