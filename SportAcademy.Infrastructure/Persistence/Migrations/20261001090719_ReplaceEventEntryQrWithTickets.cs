using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEventEntryQrWithTickets : Migration
    {
        /// <inheritdoc />
        // Entry flips from "each guest's phone scans the event's one QR code" to "staff scan each
        // guest's own ticket". The old per-phone admissions and event-wide code go away; tickets
        // are issued on demand from the event page, so nothing is backfilled.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAdmissions");

            migrationBuilder.DropIndex(
                name: "IX_Events_EntryToken",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AdmittedCount",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EntryToken",
                table: "Events");

            migrationBuilder.CreateTable(
                name: "EventTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTickets_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventTickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTickets_EventId_Number",
                table: "EventTickets",
                columns: new[] { "EventId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTickets_TenantId",
                table: "EventTickets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTickets_Token",
                table: "EventTickets",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTickets");

            migrationBuilder.AddColumn<int>(
                name: "AdmittedCount",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EntryToken",
                table: "Events",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Same backfill as AddEventEntryQrCode: every event needs its own code before the
            // unique index on EntryToken is recreated below.
            migrationBuilder.Sql(
                "UPDATE Events SET EntryToken = LOWER(CONVERT(varchar(32), CRYPT_GEN_RANDOM(16), 2)) WHERE EntryToken = '';");

            migrationBuilder.CreateTable(
                name: "EventAdmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAdmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAdmissions_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventAdmissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EntryToken",
                table: "Events",
                column: "EntryToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmissions_EventId_DeviceKey",
                table: "EventAdmissions",
                columns: new[] { "EventId", "DeviceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmissions_TenantId",
                table: "EventAdmissions",
                column: "TenantId");
        }
    }
}
