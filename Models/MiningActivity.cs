using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class MiningActivity
    {
        public int Id { get; set; }
        
        [Display(Name = "Miner")]
        public int MinerId { get; set; }
        
        [ForeignKey("MinerId")]
        public virtual Miner Miner { get; set; } = null!;
        
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; } = string.Empty; // Extraction, Processing, Transportation
        
        [Display(Name = "Mineral Type")]
        public string MineralType { get; set; } = string.Empty;
        
        [Display(Name = "Quantity (kg)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }
        
        [Display(Name = "Location")]
        public string ActivityLocation { get; set; } = string.Empty;
        
        [Display(Name = "Equipment Used")]
        public string? EquipmentUsed { get; set; }
        
        [Display(Name = "Workers Count")]
        public int WorkersCount { get; set; }
        
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }
        
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; } = "Completed"; // Planned, In Progress, Completed, Suspended
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}