using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class InfrastructureCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Navigation properties
        public virtual ICollection<InfrastructureProposal> Proposals { get; set; }
    }
}