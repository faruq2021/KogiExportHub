using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class JointVenture
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string VentureName { get; set; }
        
        [Required]
        public int InvestorId { get; set; }
        
        [Required]
        public int LocalPartnerId { get; set; } // UserProfile ID
        
        public int? OpportunityId { get; set; } // Related investment opportunity
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalInvestment { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal InvestorSharePercentage { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal LocalPartnerSharePercentage { get; set; }
        
        public string? ContractDocument { get; set; } // File path
        
        public string Status { get; set; } = "Proposed"; // Proposed, Negotiating, Agreed, Active, Completed, Terminated
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Investor Investor { get; set; }
        public virtual UserProfile LocalPartner { get; set; }
        public virtual InvestmentOpportunity? Opportunity { get; set; }
        public virtual ICollection<VentureUpdate> Updates { get; set; } = new List<VentureUpdate>();
    }
}