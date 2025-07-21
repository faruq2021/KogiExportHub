namespace KogiExportHub.Models
{
    public class HomeViewModel
    {
        public int TotalProposals { get; set; }
        public int ActiveProposals { get; set; }
        public decimal TotalFundingRequested { get; set; }
        public decimal TotalFundingApproved { get; set; }
    }

    // Add these classes back for the Infrastructure Dashboard
    public class UserProposalSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime SubmissionDate { get; set; }
    }

    public class UserFundingRequestSummary
    {
        public int Id { get; set; }
        public string ProposalTitle { get; set; }
        public decimal AmountRequested { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
    }

    // Create a separate view model for the Infrastructure Dashboard
    public class DashboardViewModel
    {
        public List<UserProposalSummary> UserProposals { get; set; } = new List<UserProposalSummary>();
        public List<UserFundingRequestSummary> UserFundingRequests { get; set; } = new List<UserFundingRequestSummary>();
        public int TotalProposals { get; set; }
        public int ActiveProposals { get; set; }
        public decimal TotalFundingRequested { get; set; }
        public decimal TotalFundingApproved { get; set; }
    }
}