using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReconciliationBypassCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BypassCodeAttempts",
                table: "TenantLimitReconciliations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BypassCodeExpiresAt",
                table: "TenantLimitReconciliations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BypassCodeHash",
                table: "TenantLimitReconciliations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasBypassedBySuperAdmin",
                table: "TenantLimitReconciliations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BypassCodeAttempts",
                table: "TenantLimitReconciliations");

            migrationBuilder.DropColumn(
                name: "BypassCodeExpiresAt",
                table: "TenantLimitReconciliations");

            migrationBuilder.DropColumn(
                name: "BypassCodeHash",
                table: "TenantLimitReconciliations");

            migrationBuilder.DropColumn(
                name: "WasBypassedBySuperAdmin",
                table: "TenantLimitReconciliations");
        }
    }
}
