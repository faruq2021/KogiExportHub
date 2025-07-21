using System;
using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class AdminDashboardViewModel
    {
        public List<Miner> PendingMiners { get; set; } = new List<Miner>();
        public List<Investor> PendingInvestors { get; set; } = new List<Investor>();
        public List<InfrastructureProposal> PendingInfrastructure { get; set; } = new List<InfrastructureProposal>();
        public List<TaxCalculation> PendingTaxReturns { get; set; } = new List<TaxCalculation>();
        public List<AdminActivity> RecentActivities { get; set; } = new List<AdminActivity>();
        
        // Properties for dashboard statistics
        public int TotalUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public int TotalInvestors { get; set; }
        public int VerifiedInvestors { get; set; }
        public int RejectedInvestors { get; set; }
        
        // Add this property to fix the count issue
        public int PendingInvestorsCount => PendingInvestors?.Count ?? 0;
        
        public List<Investor> PendingInvestorRegistrations { get; set; } = new List<Investor>();
        public List<Investor> RecentInvestorActivity { get; set; } = new List<Investor>();
        public List<UserProfile> RecentRegistrations { get; set; } = new List<UserProfile>();
        
        public int TotalPendingApprovals => PendingMiners.Count + PendingInvestors.Count + 
                                           PendingInfrastructure.Count + PendingTaxReturns.Count;
    }
}