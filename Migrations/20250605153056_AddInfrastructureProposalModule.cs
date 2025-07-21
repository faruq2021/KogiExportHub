using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KogiExportHub.Migrations
{
    /// <inheritdoc />
    public partial class AddInfrastructureProposalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfrastructureCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InfrastructureProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ProposerId = table.Column<int>(type: "int", nullable: false),
                    DocumentationUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ExpectedImpact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedBeneficiaries = table.Column<int>(type: "int", nullable: false),
                    ExpectedTimelineMonths = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfrastructureProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfrastructureProposals_InfrastructureCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "InfrastructureCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InfrastructureProposals_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InfrastructureProposals_UserProfiles_ProposerId",
                        column: x => x.ProposerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    AmountRequested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountApproved = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FundingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EquityPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RepaymentTermMonths = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FundingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsShariaCompliant = table.Column<bool>(type: "bit", nullable: false),
                    ProfitSharingTerms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingRequests_InfrastructureProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "InfrastructureProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingRequests_UserProfiles_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FundingAllocation = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VerificationDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerifierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_InfrastructureProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "InfrastructureProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_UserProfiles_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundingContributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundingRequestId = table.Column<int>(type: "int", nullable: false),
                    ContributorId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContributionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingContributions_FundingRequests_FundingRequestId",
                        column: x => x.FundingRequestId,
                        principalTable: "FundingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingContributions_UserProfiles_ContributorId",
                        column: x => x.ContributorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingContributions_ContributorId",
                table: "FundingContributions",
                column: "ContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingContributions_FundingRequestId",
                table: "FundingContributions",
                column: "FundingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingRequests_ProposalId",
                table: "FundingRequests",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingRequests_RequesterId",
                table: "FundingRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureProposals_CategoryId",
                table: "InfrastructureProposals",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureProposals_LocationId",
                table: "InfrastructureProposals",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InfrastructureProposals_ProposerId",
                table: "InfrastructureProposals",
                column: "ProposerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ProposalId",
                table: "ProjectMilestones",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_VerifierId",
                table: "ProjectMilestones",
                column: "VerifierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundingContributions");

            migrationBuilder.DropTable(
                name: "ProjectMilestones");

            migrationBuilder.DropTable(
                name: "FundingRequests");

            migrationBuilder.DropTable(
                name: "InfrastructureProposals");

            migrationBuilder.DropTable(
                name: "InfrastructureCategories");
        }
    }
}
