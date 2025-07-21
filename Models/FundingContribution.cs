using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class FundingContribution
    {
        public int Id { get; set; }
        
        [Required]
        public int FundingRequestId { get; set; }
        
        [Required]
        public int ContributorId { get; set; } // UserProfile ID of the contributor
        
        [Required]
        public decimal Amount { get; set; }
        
        public string? TransactionReference { get; set; } // Made nullable
        
        public DateTime ContributionDate { get; set; }
        
        public string Status { get; set; } // Pending, Completed, Refunded
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual FundingRequest FundingRequest { get; set; }
        public virtual UserProfile Contributor { get; set; }
    }
}
