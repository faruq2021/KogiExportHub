using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class InvestmentOpportunity
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        public string Description { get; set; }
        public string Sector { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequiredInvestment { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal ExpectedROI { get; set; }
        
        public int InvestmentPeriodMonths { get; set; }
        public string RiskLevel { get; set; }
        
        public string? BusinessPlan { get; set; }
        public string? FinancialProjections { get; set; }
        
        public int LocationId { get; set; }
        public int? LocalPartnerId { get; set; }
        public int? InvestorId { get; set; }
        
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Location Location { get; set; }
        public virtual UserProfile? LocalPartner { get; set; }
        public virtual Investor? Investor { get; set; }
        public virtual ICollection<JointVenture> JointVentures { get; set; } = new List<JointVenture>();
    }
}