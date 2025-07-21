using System;
using System.Collections.Generic;

namespace KogiExportHub.Models
{
    public class SellerDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalSales { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
    }
}