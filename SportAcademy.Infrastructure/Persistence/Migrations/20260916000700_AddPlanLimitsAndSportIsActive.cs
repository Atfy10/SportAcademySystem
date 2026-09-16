using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanLimitsAndSportIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sports",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "PlanLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionPlanId = table.Column<int>(type: "int", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanLimits_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantLimitOverrides",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxCount = table.Column<int>(type: "int", nullable: true),
                    SetBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SetAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLimitOverrides", x => new { x.TenantId, x.ResourceKey });
                    table.ForeignKey(
                        name: "FK_TenantLimitOverrides_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanLimits_SubscriptionPlanId_ResourceKey",
                table: "PlanLimits",
                columns: new[] { "SubscriptionPlanId", "ResourceKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanLimits");

            migrationBuilder.DropTable(
                name: "TenantLimitOverrides");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sports");
        }
    }
}
