using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KogiExportHub.Models
{
    public class Miner
    {
        public int Id { get; set; }
        
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mining Type is required")]
        [Display(Name = "Mining Type")]
        public string MiningType { get; set; } = string.Empty; // Gold, Tin, Columbite, etc.
        
        [Display(Name = "License Status")]
        public string LicenseStatus { get; set; } = "Pending"; // Pending, Active, Suspended, Expired
        
        [Display(Name = "Issue Date")]
        public DateTime? IssueDate { get; set; }
        
        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }
        
        [Display(Name = "Location")]
        public int? LocationId { get; set; }
        
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }
        
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Notes")]
public string? Notes { get; set; }
        
        // Navigation properties
        public virtual ICollection<MiningActivity> MiningActivities { get; set; } = new List<MiningActivity>();
        public virtual ICollection<EnvironmentalCompliance> EnvironmentalCompliances { get; set; } = new List<EnvironmentalCompliance>();
    }
}