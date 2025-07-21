using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class InvestorMessage
    {
        public int Id { get; set; }
        
        public int? SenderId { get; set; } // Investor ID
        
        public int? RecipientId { get; set; } // Investor ID
        
        public int? LocalUserId { get; set; } // UserProfile ID for local users
        
        [Required]
        public string Subject { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        
        public string? AttachmentPath { get; set; }
        
        // Navigation properties
        public virtual Investor? Sender { get; set; }
        public virtual Investor? Recipient { get; set; }
        public virtual UserProfile? LocalUser { get; set; }
    }
}