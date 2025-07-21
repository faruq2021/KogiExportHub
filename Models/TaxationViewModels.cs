using System;
using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class TaxationDashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RevenueCategoryViewModel> RevenueByCategory { get; set; } = new List<RevenueCategoryViewModel>();
        public List<GovernmentRevenue> RecentRevenues { get; set; } = new List<GovernmentRevenue>();
        public List<MonthlyRevenueViewModel> MonthlyTrend { get; set; } = new List<MonthlyRevenueViewModel>();
    }
    
    public class RevenueCategoryViewModel
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
    
    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}