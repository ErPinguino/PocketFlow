using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddPiggyBankContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PiggyBankContributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PiggyBankId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthlyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiggyBankContributions", x => x.Id);
                    table.CheckConstraint("CK_PiggyBankContribution_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_PiggyBankContributions_MonthlyPlans_MonthlyPlanId",
                        column: x => x.MonthlyPlanId,
                        principalTable: "MonthlyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PiggyBankContributions_PiggyBanks_PiggyBankId",
                        column: x => x.PiggyBankId,
                        principalTable: "PiggyBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PiggyBankContributions_MonthlyPlanId",
                table: "PiggyBankContributions",
                column: "MonthlyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PiggyBankContributions_PiggyBankId_MonthlyPlanId_Type",
                table: "PiggyBankContributions",
                columns: new[] { "PiggyBankId", "MonthlyPlanId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PiggyBankContributions");
        }
    }
}
