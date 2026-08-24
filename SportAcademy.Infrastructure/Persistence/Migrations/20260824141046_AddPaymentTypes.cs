using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // Replaces the hardcoded PaymentMethod enum (Cash/Online) with a real, tenant-owned
    // PaymentTypes table an Admin can manage (add/rename/deactivate/set-default) - see
    // Domain.Entities.PaymentType. Payments.Method (an nvarchar column holding the enum's
    // string-converted value, e.g. "Cash") is replaced by Payments.PaymentTypeId (FK).
    //
    // Every existing tenant gets a seeded "Cash" (flagged default, matching the old
    // UpdatePaymentStatusCommandHandler's hardcoded PaymentMethod.Cash) and "Online" row before
    // existing Payments are backfilled by matching TenantId + Name against the old Method text.
    //
    // IRREVERSIBLE: Down() backfills a "Method" column from each payment's PaymentType.Name
    // before dropping the table, so the string values themselves aren't lost - but the
    // PaymentMethod C# enum this migration's Up() replaces is deleted from the codebase
    // entirely, and any payment type an Admin added/renamed after this migration ran has no
    // enum member to round-trip back to. Take a database backup before applying this migration
    // to any environment with real data.
    public partial class AddPaymentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_TenantId_Name",
                table: "PaymentTypes",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            // One Cash (default) + one Online row per existing tenant - matches every value the
            // old PaymentMethod enum could hold, so the backfill below always finds a match.
            migrationBuilder.Sql(@"
                INSERT INTO [PaymentTypes] ([Name], [IsActive], [IsDefault], [TenantId], [CreatedAt])
                SELECT N'Cash', 1, 1, [Id], GETUTCDATE() FROM [Tenants]
                UNION ALL
                SELECT N'Online', 1, 0, [Id], GETUTCDATE() FROM [Tenants];
            ");

            // Nullable for now - populated by the backfill below, then locked to NOT NULL once
            // every row has a value.
            migrationBuilder.AddColumn<int>(
                name: "PaymentTypeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE p
                SET p.[PaymentTypeId] = pt.[Id]
                FROM [Payments] p
                JOIN [PaymentTypes] pt ON pt.[TenantId] = p.[TenantId] AND pt.[Name] = p.[Method];
            ");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentTypeId",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentTypes_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            // Best-effort: recover the string value from each payment's current PaymentType
            // rather than losing it outright - but see the IRREVERSIBLE note on this class for
            // why this isn't a true rollback.
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.[Method] = pt.[Name]
                FROM [Payments] p
                JOIN [PaymentTypes] pt ON pt.[Id] = p.[PaymentTypeId];
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentTypes_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "Payments");
        }
    }
}
