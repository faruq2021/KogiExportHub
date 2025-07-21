using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KogiExportHub.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentOpportunityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId",
                table: "InvestmentOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_Locations_LocationId",
                table: "InvestmentOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_UserProfiles_LocalPartnerId",
                table: "InvestmentOpportunities");

            migrationBuilder.AddColumn<int>(
                name: "InvestorId1",
                table: "InvestmentOpportunities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentOpportunities_InvestorId1",
                table: "InvestmentOpportunities",
                column: "InvestorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId",
                table: "InvestmentOpportunities",
                column: "InvestorId",
                principalTable: "Investors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId1",
                table: "InvestmentOpportunities",
                column: "InvestorId1",
                principalTable: "Investors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_Locations_LocationId",
                table: "InvestmentOpportunities",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_UserProfiles_LocalPartnerId",
                table: "InvestmentOpportunities",
                column: "LocalPartnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId",
                table: "InvestmentOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId1",
                table: "InvestmentOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_Locations_LocationId",
                table: "InvestmentOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentOpportunities_UserProfiles_LocalPartnerId",
                table: "InvestmentOpportunities");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentOpportunities_InvestorId1",
                table: "InvestmentOpportunities");

            migrationBuilder.DropColumn(
                name: "InvestorId1",
                table: "InvestmentOpportunities");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_Investors_InvestorId",
                table: "InvestmentOpportunities",
                column: "InvestorId",
                principalTable: "Investors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_Locations_LocationId",
                table: "InvestmentOpportunities",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentOpportunities_UserProfiles_LocalPartnerId",
                table: "InvestmentOpportunities",
                column: "LocalPartnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }
    }
}
