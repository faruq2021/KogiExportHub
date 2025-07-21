using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KogiExportHub.Migrations
{
    /// <inheritdoc />
    public partial class AddDisbursementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisbursementDate",
                table: "FundingRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisbursementReference",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisbursementStatus",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisbursementTransferId",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientAccountName",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientAccountNumber",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientBankCode",
                table: "FundingRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisbursementDate",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "DisbursementReference",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "DisbursementStatus",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "DisbursementTransferId",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "RecipientAccountName",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "RecipientAccountNumber",
                table: "FundingRequests");

            migrationBuilder.DropColumn(
                name: "RecipientBankCode",
                table: "FundingRequests");
        }
    }
}
