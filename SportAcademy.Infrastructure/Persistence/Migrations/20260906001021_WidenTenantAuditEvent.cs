using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WidenTenantAuditEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AfterJson",
                table: "TenantAuditEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeJson",
                table: "TenantAuditEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "TenantAuditEvents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "TenantAuditEvents",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "TenantAuditEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedByUserId",
                table: "TenantAuditEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TenantAuditEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "TenantAuditEvents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Backfill for any row that predates this migration: Outcome's DB-level default
            // above ("") satisfies the new NOT NULL constraint but doesn't match any
            // AuditOutcome member name, which the string converter would reject on read. Every
            // pre-existing row was logged by code that only ever ran on `if (result.IsSuccess)`
            // (see the old TenantsController.LogAsync), so "Succeeded" is not a guess - it's
            // what already-true condition every one of those rows satisfies.
            //
            // PerformedByUserId has no equivalent backfill: the old schema only ever stored a
            // display-name string, with no user id alongside it, so there is nothing to recover
            // it from. Guid.Empty stays as an honest "actor unknown - row predates id tracking"
            // sentinel rather than a fabricated identity.
            migrationBuilder.Sql(
                "UPDATE [TenantAuditEvents] SET [Outcome] = 'Succeeded' WHERE [Outcome] = ''");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditEvents_EventType",
                table: "TenantAuditEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuditEvents_PerformedByUserId",
                table: "TenantAuditEvents",
                column: "PerformedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantAuditEvents_EventType",
                table: "TenantAuditEvents");

            migrationBuilder.DropIndex(
                name: "IX_TenantAuditEvents_PerformedByUserId",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "AfterJson",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "BeforeJson",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TenantAuditEvents");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "TenantAuditEvents");
        }
    }
}
