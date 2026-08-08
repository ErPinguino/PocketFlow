using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddPaydayToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaycheckConfirmedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPaycheckConfirmedAt",
                table: "Accounts");
        }
    }
}
