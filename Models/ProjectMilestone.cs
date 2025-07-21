using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class ProjectMilestone
    {
        public int Id { get; set; }
        
        [Required]
        public int ProposalId { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public DateTime? TargetCompletionDate { get; set; }
        
        public DateTime? ActualCompletionDate { get; set; }
        
        public string Status { get; set; } // Pending, InProgress, Completed, Delayed
        
        public decimal? FundingAllocation { get; set; } // Amount allocated for this milestone
        
        public string VerificationDocumentUrl { get; set; } // URL to uploaded verification document
        
        public string VerificationStatus { get; set; } // Pending, Verified, Rejected
        
        public int? VerifierId { get; set; } // UserProfile ID of the verifier
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual InfrastructureProposal Proposal { get; set; }
        public virtual UserProfile Verifier { get; set; }
    }
}