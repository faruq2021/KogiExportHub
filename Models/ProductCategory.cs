using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultUnit { get; set; } // Default unit of measurement
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; }
    }
}