using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomPlansAndLeadPlanSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerTenantId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedPlanSpecJson",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantLimitReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggeredByPlanId = table.Column<int>(type: "int", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiredResourcesJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLimitReconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantLimitReconciliations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantLimitReconciliations_CompletedAt_DeadlineAt",
                table: "TenantLimitReconciliations",
                columns: new[] { "CompletedAt", "DeadlineAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantLimitReconciliations_TenantId_CompletedAt",
                table: "TenantLimitReconciliations",
                columns: new[] { "TenantId", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantLimitReconciliations");

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "OwnerTenantId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "RequestedPlanSpecJson",
                table: "Leads");
        }
    }
}
