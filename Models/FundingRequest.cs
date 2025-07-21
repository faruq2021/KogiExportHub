using System;
using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class FundingRequest
    {
        public int Id { get; set; }
        
        [Required]
        public int ProposalId { get; set; }
        
        [Required]
        public int RequesterId { get; set; } // UserProfile ID of the requester
        
        [Required]
        public decimal AmountRequested { get; set; }
        
        public decimal? AmountApproved { get; set; }
        
        [Required]
        public string FundingType { get; set; } // Grant, Loan, Investment, PublicPrivatePartnership
        
        public decimal? InterestRate { get; set; } // For loans
        
        public decimal? EquityPercentage { get; set; } // For investments
        
        public int? RepaymentTermMonths { get; set; } // For loans
        
        public string Status { get; set; } // Pending, Approved, Rejected, Funded, Completed
        
        public string RejectionReason { get; set; }
        
        public DateTime SubmissionDate { get; set; }
        
        public string? AdminComments { get; set; }
        public DateTime? ApprovalDate { get; set; }
        
        public DateTime? FundingDate { get; set; }
        
        public bool IsShariaCompliant { get; set; }
        
        public string ProfitSharingTerms { get; set; } // For Sharia-compliant funding
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual InfrastructureProposal Proposal { get; set; }
        public virtual UserProfile Requester { get; set; }
        public virtual ICollection<FundingContribution> Contributions { get; set; }
    
        // Disbursement fields
        public string? RecipientAccountNumber { get; set; }
        public string? RecipientBankCode { get; set; }
        public string? RecipientAccountName { get; set; }
        public string? DisbursementTransferId { get; set; }
        public string? DisbursementReference { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public string? DisbursementStatus { get; set; } // Pending, Completed, Failed
    }
}