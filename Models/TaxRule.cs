using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class TaxRule
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TaxType { get; set; } // VAT, Income, Export, Import, etc.
        
        [Required]
        public decimal Rate { get; set; } // Tax rate as percentage
        
        public decimal MinAmount { get; set; } // Minimum amount for tax to apply
        public decimal MaxAmount { get; set; } // Maximum amount for tax calculation
        
        [StringLength(100)]
        public string ApplicableCategory { get; set; } // Product category or transaction type
        
        [StringLength(50)]
        public string ApplicableLocation { get; set; } // Location where tax applies
        
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}