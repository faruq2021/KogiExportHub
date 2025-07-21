using System;
using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class MiningDashboardViewModel
    {
        public int TotalMiners { get; set; }
        public int ActiveMiners { get; set; }
        public int PendingApplications { get; set; }
        public int TotalActivities { get; set; }
        public int ComplianceIssues { get; set; }
        public int ActiveLicenses { get; set; }
        public int PendingLicenses { get; set; }
        public List<Miner> RecentMiners { get; set; } = new List<Miner>();
        public List<MiningActivity> RecentActivities { get; set; } = new List<MiningActivity>();
        public List<EnvironmentalCompliance> RecentCompliance { get; set; } = new List<EnvironmentalCompliance>();
        public List<MinerStatusViewModel> MinersByStatus { get; set; } = new List<MinerStatusViewModel>();
        public List<ActivityTypeViewModel> ActivitiesByType { get; set; } = new List<ActivityTypeViewModel>();
    }
    
    public class MinerStatusViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    
    public class ActivityTypeViewModel
    {
        public string ActivityType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalQuantity { get; set; }
    }
    
    public class MinerRegistrationViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string MiningType { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
    }
}