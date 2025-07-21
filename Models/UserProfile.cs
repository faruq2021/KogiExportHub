using Microsoft.AspNetCore.Identity;

namespace KogiExportHub.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; }
        public int? LocationId { get; set; }
        
        // Additional properties for business users
        public string? BusinessName { get; set; }
        public string? BusinessDescription { get; set; }
        
        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";
        
        // Flutterwave Integration Fields
        public string? FlutterwaveSubaccountId { get; set; }
        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public bool IsPaymentSetupComplete { get; set; } = false;
        
        public virtual Location Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual IdentityUser User { get; set; }
    }
}