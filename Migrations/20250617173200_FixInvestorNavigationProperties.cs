using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KogiExportHub.Migrations
{
    /// <inheritdoc />
    public partial class FixInvestorNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessDescription",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Investors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InvestmentCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredSectors = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerificationDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentOpportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredInvestment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedROI = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    InvestmentPeriodMonths = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialProjections = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocalPartnerId = table.Column<int>(type: "int", nullable: true),
                    InvestorId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentOpportunities_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvestmentOpportunities_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestmentOpportunities_UserProfiles_LocalPartnerId",
                        column: x => x.LocalPartnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvestorMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(type: "int", nullable: true),
                    RecipientId = table.Column<int>(type: "int", nullable: true),
                    LocalUserId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestorMessages_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvestorMessages_Investors_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Investors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestorMessages_Investors_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Investors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestorMessages_UserProfiles_LocalUserId",
                        column: x => x.LocalUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JointVentures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InvestorId = table.Column<int>(type: "int", nullable: false),
                    LocalPartnerId = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: true),
                    TotalInvestment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestorSharePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LocalPartnerSharePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ContractDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JointVentures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JointVentures_InvestmentOpportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "InvestmentOpportunities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JointVentures_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JointVentures_UserProfiles_LocalPartnerId",
                        column: x => x.LocalPartnerId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentureUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JointVentureId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialUpdate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ROIToDate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SupportingDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentureUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentureUpdates_JointVentures_JointVentureId",
                        column: x => x.JointVentureId,
                        principalTable: "JointVentures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VentureUpdates_UserProfiles_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOpportunities_InvestorId",
                table: "InvestmentOpportunities",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOpportunities_LocalPartnerId",
                table: "InvestmentOpportunities",
                column: "LocalPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOpportunities_LocationId",
                table: "InvestmentOpportunities",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorMessages_InvestorId",
                table: "InvestorMessages",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorMessages_LocalUserId",
                table: "InvestorMessages",
                column: "LocalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorMessages_RecipientId",
                table: "InvestorMessages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorMessages_SenderId",
                table: "InvestorMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_JointVentures_InvestorId",
                table: "JointVentures",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_JointVentures_LocalPartnerId",
                table: "JointVentures",
                column: "LocalPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_JointVentures_OpportunityId",
                table: "JointVentures",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_VentureUpdates_JointVentureId",
                table: "VentureUpdates",
                column: "JointVentureId");

            migrationBuilder.CreateIndex(
                name: "IX_VentureUpdates_UpdatedByUserId",
                table: "VentureUpdates",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestorMessages");

            migrationBuilder.DropTable(
                name: "VentureUpdates");

            migrationBuilder.DropTable(
                name: "JointVentures");

            migrationBuilder.DropTable(
                name: "InvestmentOpportunities");

            migrationBuilder.DropTable(
                name: "Investors");

            migrationBuilder.DropColumn(
                name: "BusinessDescription",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "UserProfiles");
        }
    }
}
