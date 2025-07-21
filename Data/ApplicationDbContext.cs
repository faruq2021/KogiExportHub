using KogiExportHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace KogiExportHub.Data
{

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // Infrastructure Proposal & Funding Module
        public DbSet<InfrastructureCategory> InfrastructureCategories { get; set; }
        public DbSet<InfrastructureProposal> InfrastructureProposals { get; set; }
        public DbSet<FundingRequest> FundingRequests { get; set; }
        public DbSet<FundingContribution> FundingContributions { get; set; }
        public DbSet<ProjectMilestone> ProjectMilestones { get; set; }

        // Taxation Module
        public DbSet<TaxRule> TaxRules { get; set; }
        public DbSet<TaxCalculation> TaxCalculations { get; set; }
        public DbSet<GovernmentRevenue> GovernmentRevenues { get; set; }
        public DbSet<TaxReceipt> TaxReceipts { get; set; }

        // Investment Module
        public DbSet<Investor> Investors { get; set; }
        public DbSet<InvestmentOpportunity> InvestmentOpportunities { get; set; }
        public DbSet<JointVenture> JointVentures { get; set; }
        public DbSet<InvestorMessage> InvestorMessages { get; set; }
        public DbSet<VentureUpdate> VentureUpdates { get; set; }

        // Mining Module
        public DbSet<Miner> Miners { get; set; }
        public DbSet<MiningActivity> MiningActivities { get; set; }
        public DbSet<EnvironmentalCompliance> EnvironmentalCompliances { get; set; }
        
        // Admin Module
        public DbSet<AdminActivity> AdminActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Infrastructure Proposal & Funding Module decimal precision
            modelBuilder.Entity<InfrastructureProposal>()
                .Property(p => p.EstimatedCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FundingRequest>()
                .Property(f => f.AmountRequested)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FundingRequest>()
                .Property(f => f.AmountApproved)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FundingRequest>()
                .Property(f => f.InterestRate)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<FundingRequest>()
                .Property(f => f.EquityPercentage)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<FundingContribution>()
                .Property(f => f.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProjectMilestone>()
                .Property(p => p.FundingAllocation)
                .HasColumnType("decimal(18,2)");

            // Investment Opportunity decimal precision
            modelBuilder.Entity<InvestmentOpportunity>()
                .Property(i => i.RequiredInvestment)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InvestmentOpportunity>()
                .Property(i => i.ExpectedROI)
                .HasColumnType("decimal(5,2)");

            // Taxation decimal precision
            modelBuilder.Entity<TaxRule>()
                .Property(t => t.Rate)
                .HasColumnType("decimal(5,2)");
                
            modelBuilder.Entity<TaxRule>()
                .Property(t => t.MinAmount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<TaxRule>()
                .Property(t => t.MaxAmount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<TaxCalculation>()
                .Property(t => t.BaseAmount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<TaxCalculation>()
                .Property(t => t.TaxRate)
                .HasColumnType("decimal(5,2)");
                
            modelBuilder.Entity<TaxCalculation>()
                .Property(t => t.TaxAmount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<GovernmentRevenue>()
                .Property(g => g.Amount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<TaxReceipt>()
                .Property(t => t.TotalTaxAmount)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<TaxReceipt>()
                .Property(t => t.TransactionAmount)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Location)
                .WithMany(l => l.Products)
                .HasForeignKey(p => p.LocationId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Product)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.ProductId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Buyer)
                .WithMany()
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Infrastructure Proposal & Funding Module relationships
            modelBuilder.Entity<InfrastructureProposal>()
                .HasOne(p => p.Proposer)
                .WithMany()
                .HasForeignKey(p => p.ProposerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InfrastructureProposal>()
                .HasOne(p => p.Location)
                .WithMany()
                .HasForeignKey(p => p.LocationId);

            modelBuilder.Entity<InfrastructureProposal>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Proposals)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<FundingRequest>()
                .HasOne(f => f.Proposal)
                .WithMany(p => p.FundingRequests)
                .HasForeignKey(f => f.ProposalId);

            modelBuilder.Entity<FundingRequest>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FundingContribution>()
                .HasOne(f => f.FundingRequest)
                .WithMany(r => r.Contributions)
                .HasForeignKey(f => f.FundingRequestId);

            modelBuilder.Entity<FundingContribution>()
                .HasOne(f => f.Contributor)
                .WithMany()
                .HasForeignKey(f => f.ContributorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(m => m.Proposal)
                .WithMany(p => p.Milestones)
                .HasForeignKey(m => m.ProposalId);

            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(m => m.Verifier)
                .WithMany()
                .HasForeignKey(m => m.VerifierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Investor Message relationships
            modelBuilder.Entity<InvestorMessage>()
                .HasOne(m => m.Sender)
                .WithMany(i => i.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvestorMessage>()
                .HasOne(m => m.Recipient)
                .WithMany(i => i.ReceivedMessages)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Joint Venture relationships
            modelBuilder.Entity<JointVenture>()
                .HasOne(jv => jv.Investor)
                .WithMany(i => i.JointVentures)
                .HasForeignKey(jv => jv.InvestorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JointVenture>()
                .HasOne(jv => jv.LocalPartner)
                .WithMany()
                .HasForeignKey(jv => jv.LocalPartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Investment Opportunity relationships
            modelBuilder.Entity<InvestmentOpportunity>()
                .HasOne(i => i.Location)
                .WithMany()
                .HasForeignKey(i => i.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvestmentOpportunity>()
                .HasOne(i => i.LocalPartner)
                .WithMany()
                .HasForeignKey(i => i.LocalPartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvestmentOpportunity>()
                .HasOne(i => i.Investor)
                .WithMany()
                .HasForeignKey(i => i.InvestorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mining configurations
            modelBuilder.Entity<Miner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicenseNumber).HasMaxLength(50);
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.MiningType).HasMaxLength(100);
                entity.Property(e => e.LicenseStatus).HasMaxLength(50);
            
                entity.HasOne(e => e.Location)
                    .WithMany()
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            modelBuilder.Entity<MiningActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActivityType).HasMaxLength(100);
                entity.Property(e => e.MineralType).HasMaxLength(100);
                entity.Property(e => e.ActivityLocation).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Quantity).HasPrecision(10, 2);
            
                entity.HasOne(e => e.Miner)
                    .WithMany(m => m.MiningActivities)
                    .HasForeignKey(e => e.MinerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<EnvironmentalCompliance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ComplianceType).HasMaxLength(100);
                entity.Property(e => e.ComplianceStatus).HasMaxLength(50);
                entity.Property(e => e.InspectorName).HasMaxLength(200);
                entity.Property(e => e.CertificateNumber).HasMaxLength(100);
                entity.Property(e => e.FineStatus).HasMaxLength(50);
                entity.Property(e => e.FineAmount).HasPrecision(10, 2);
            
                entity.HasOne(e => e.Miner)
                    .WithMany(m => m.EnvironmentalCompliances)
                    .HasForeignKey(e => e.MinerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Tax relationships
            modelBuilder.Entity<TaxCalculation>()
                .HasOne(tc => tc.Transaction)
                .WithMany()
                .HasForeignKey(tc => tc.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<TaxCalculation>()
                .HasOne(tc => tc.TaxRule)
                .WithMany()
                .HasForeignKey(tc => tc.TaxRuleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<GovernmentRevenue>()
                .HasOne(gr => gr.Transaction)
                .WithMany()
                .HasForeignKey(gr => gr.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<GovernmentRevenue>()
                .HasOne(gr => gr.TaxCalculation)
                .WithMany()
                .HasForeignKey(gr => gr.TaxCalculationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<TaxReceipt>()
                .HasOne(tr => tr.Transaction)
                .WithMany()
                .HasForeignKey(tr => tr.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<TaxReceipt>()
                .HasOne(tr => tr.Payer)
                .WithMany()
                .HasForeignKey(tr => tr.PayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Admin Activity configuration
            modelBuilder.Entity<AdminActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.AdminUser).HasMaxLength(256).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
            });
        }
    }
}