using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class InfrastructureProposal
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public decimal EstimatedCost { get; set; }
        
        [Required]
        public int LocationId { get; set; }
        
        public int ProposerId { get; set; } // Remove [Required] - set by controller
        
        public string DocumentationUrl { get; set; } // Remove [Required] - optional for drafts
        
        public string Status { get; set; } // Remove [Required] - set by controller
        
        public DateTime SubmissionDate { get; set; }
        
        public DateTime? ApprovalDate { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public string ExpectedImpact { get; set; }
        
        public int? ExpectedBeneficiaries { get; set; } // Make nullable
        
        public int? ExpectedTimelineMonths { get; set; } // Make nullable
        
        public DateTime CreatedAt { get; set; }
        
        // Add missing CreatedDate property (alias for CreatedAt)
        public DateTime CreatedDate 
        { 
            get => CreatedAt; 
            set => CreatedAt = value; 
        }
        
        public DateTime UpdatedAt { get; set; }
        
        // Add missing Notes property
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        // Navigation properties - these should not be required in model binding
        public virtual Location Location { get; set; }
        public virtual UserProfile Proposer { get; set; }
        public virtual InfrastructureCategory Category { get; set; }
        public virtual ICollection<FundingRequest> FundingRequests { get; set; } = new List<FundingRequest>();
        public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    }
}