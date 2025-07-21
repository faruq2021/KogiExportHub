using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class AdminActivity
    {
        public int Id { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string AdminUser { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}