using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventEntryQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            // Events booked before this migration get their own entry code - 128 random bits as
            // 32 lowercase hex characters, the same shape EventEntryRules.NewToken() makes -
            // before the unique index below, which would otherwise reject them all sharing "".
            migrationBuilder.Sql(
                "UPDATE Events SET EntryToken = LOWER(CONVERT(varchar(32), CRYPT_GEN_RANDOM(16), 2)) WHERE EntryToken = '';");

            migrationBuilder.CreateTable(
                name: "EventAdmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    DeviceKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
