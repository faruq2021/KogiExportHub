using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class VentureUpdate
    {
        public int Id { get; set; }
        
        [Required]
        public int JointVentureId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinancialUpdate { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ROIToDate { get; set; }
        
        public string? SupportingDocuments { get; set; } // File paths
        
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        
        public int UpdatedBy { get; set; } // UserProfile ID
        
        // Navigation properties
        public virtual JointVenture JointVenture { get; set; }
        public virtual UserProfile UpdatedByUser { get; set; }
    }
}