using System;

namespace KogiExportHub.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Foreign key to Product
        public int BuyerId { get; set; } // Foreign key to UserProfile
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Pending, Completed, Cancelled
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual UserProfile Buyer { get; set; }
    }
}