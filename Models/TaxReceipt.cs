using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class TaxReceipt
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; }
        
        public int TransactionId { get; set; }
        public int PayerId { get; set; } // UserProfile ID
        
        [Required]
        public decimal TotalTaxAmount { get; set; }
        
        [Required]
        public decimal TransactionAmount { get; set; }
        
        [Required]
        public DateTime IssuedDate { get; set; }
        
        [StringLength(200)]
        public string PayerName { get; set; }
        
        [StringLength(200)]
        public string PayerEmail { get; set; }
        
        [StringLength(500)]
        public string TaxBreakdown { get; set; } // JSON string of tax details
        
        [StringLength(50)]
        public string Status { get; set; } = "Issued"; // Issued, Cancelled, Amended
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Transaction Transaction { get; set; }
        public virtual UserProfile Payer { get; set; }
    }
}