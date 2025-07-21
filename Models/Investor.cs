using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class Investor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string Phone { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Country { get; set; }
        
        [StringLength(500)]
        public string BusinessDescription { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvestmentCapacity { get; set; }
        
        [StringLength(200)]
        public string PreferredSectors { get; set; }
        
        public string VerificationStatus { get; set; } = "Pending";
        
        // Add missing Status property (alias for VerificationStatus)
        public string Status 
        { 
            get => VerificationStatus; 
            set => VerificationStatus = value; 
        }
        
        public string? VerificationDocuments { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        public DateTime? VerificationDate { get; set; }
        
        // Add missing ApprovalDate property
        public DateTime? ApprovalDate 
        { 
            get => VerificationDate; 
            set => VerificationDate = value; 
        }
        
        [StringLength(500)]
        public string? AdminComments { get; set; }
        
        // Add missing Notes property (alias for AdminComments)
        public string? Notes 
        { 
            get => AdminComments; 
            set => AdminComments = value; 
        }
        
        [StringLength(500)]
        public string? RejectionReason { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<InvestmentOpportunity> InvestmentOpportunities { get; set; } = new List<InvestmentOpportunity>();
        public virtual ICollection<JointVenture> JointVentures { get; set; } = new List<JointVenture>();
        public virtual ICollection<InvestorMessage> Messages { get; set; } = new List<InvestorMessage>();
        public virtual ICollection<InvestorMessage> SentMessages { get; set; } = new List<InvestorMessage>();
        public virtual ICollection<InvestorMessage> ReceivedMessages { get; set; } = new List<InvestorMessage>();
    }
}