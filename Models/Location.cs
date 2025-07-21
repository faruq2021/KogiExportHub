using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}