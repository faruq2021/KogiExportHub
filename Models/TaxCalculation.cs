using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class TaxCalculation
    {
        public int Id { get; set; }
        
        public int TransactionId { get; set; }
        public int TaxRuleId { get; set; }
        
        [Required]
        public decimal BaseAmount { get; set; } // Amount on which tax is calculated
        
        [Required]
        public decimal TaxRate { get; set; } // Rate used for calculation
        
        [Required]
        public decimal TaxAmount { get; set; } // Calculated tax amount
        
        [StringLength(50)]
        public string TaxType { get; set; }
        
        // Add missing Status property
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        public DateTime CalculatedAt { get; set; }
        
        // Add missing CalculationDate property (alias for CalculatedAt)
        public DateTime CalculationDate 
        { 
            get => CalculatedAt; 
            set => CalculatedAt = value; 
        }
        
        // Navigation properties
        public virtual Transaction Transaction { get; set; }
        public virtual TaxRule TaxRule { get; set; }
    }
}