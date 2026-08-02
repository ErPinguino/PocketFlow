using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFlow.Migrations
{
    /// <inheritdoc />
    public partial class AlignDomainWithOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TreatsBudget",
                table: "MonthlyPlans",
                newName: "WhimBudget");

            migrationBuilder.RenameColumn(
                name: "TotalIncome",
                table: "MonthlyPlans",
                newName: "Income");

            migrationBuilder.RenameColumn(
                name: "FreeMoney",
                table: "MonthlyPlans",
                newName: "FreePocketAmount");

            migrationBuilder.RenameColumn(
                name: "AllocatedToPiggyBanks",
                table: "MonthlyPlans",
                newName: "TotalSavings");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "PiggyBanks",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "PiggyBanks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "WhimBudget",
                table: "MonthlyPlans",
                newName: "TreatsBudget");

            migrationBuilder.RenameColumn(
                name: "TotalSavings",
                table: "MonthlyPlans",
                newName: "AllocatedToPiggyBanks");

            migrationBuilder.RenameColumn(
                name: "Income",
                table: "MonthlyPlans",
                newName: "TotalIncome");

            migrationBuilder.RenameColumn(
                name: "FreePocketAmount",
                table: "MonthlyPlans",
                newName: "FreeMoney");
        }
    }
}
