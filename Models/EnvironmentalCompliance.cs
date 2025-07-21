using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class EnvironmentalCompliance
    {
        public int Id { get; set; }
        
        [Display(Name = "Miner")]
        public int MinerId { get; set; }
        
        [ForeignKey("MinerId")]
        public virtual Miner Miner { get; set; } = null!;
        
        [Display(Name = "Compliance Type")]
        public string ComplianceType { get; set; } = string.Empty; // Environmental Impact Assessment, Waste Management, Water Quality, etc.
        
        [Display(Name = "Assessment Date")]
        public DateTime AssessmentDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Inspection Date")]
        public DateTime InspectionDate { get; set; } = DateTime.Now;

        [Display(Name = "Compliance Status")]
        public string ComplianceStatus { get; set; } = "Pending"; // Compliant, Non-Compliant, Pending Review, Under Investigation
        
        [Display(Name = "Inspector Name")]
        public string? InspectorName { get; set; }
        
        [Display(Name = "Findings")]
        public string? Findings { get; set; }
        
        [Display(Name = "Recommendations")]
        public string? Recommendations { get; set; }
        
        [Display(Name = "Corrective Actions Required")]
        public string? CorrectiveActions { get; set; }
        
        [Display(Name = "Due Date for Corrections")]
        public DateTime? CorrectionDueDate { get; set; }
        
        [Display(Name = "Follow-up Date")]
        public DateTime? FollowUpDate { get; set; }
        
        [Display(Name = "Certificate Number")]
        public string? CertificateNumber { get; set; }
        
        [Display(Name = "Certificate Expiry")]
        public DateTime? CertificateExpiry { get; set; }
        
        [Display(Name = "Fine Amount")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FineAmount { get; set; }
        
        [Display(Name = "Fine Status")]
        public string? FineStatus { get; set; } // Pending, Paid, Waived
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}