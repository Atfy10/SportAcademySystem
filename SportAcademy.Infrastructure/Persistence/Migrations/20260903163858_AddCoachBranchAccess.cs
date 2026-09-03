using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachBranchAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoachBranchAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoachId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachBranchAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachBranchAccesses_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachBranchAccesses_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachBranchAccesses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachBranchAccesses_BranchId",
                table: "CoachBranchAccesses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachBranchAccesses_CoachId_BranchId",
                table: "CoachBranchAccesses",
                columns: new[] { "CoachId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachBranchAccesses_TenantId",
                table: "CoachBranchAccesses",
                column: "TenantId");

            // Backfill: give every existing coach a single branch grant matching their current
            // employer's branch, so removing Employee.BranchId from coach-branch authorization
            // doesn't change anyone's visible coach assignments the moment this migration runs -
            // admins add more branches via the coach's "Manage branches" action going forward.
            migrationBuilder.Sql(@"
                INSERT INTO CoachBranchAccesses (TenantId, CoachId, BranchId, CreatedAt)
                SELECT c.TenantId, c.EmployeeId, e.BranchId, GETUTCDATE()
                FROM Coaches c
                INNER JOIN Employees e ON e.Id = c.EmployeeId
                WHERE c.IsDeleted = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachBranchAccesses");
        }
    }
}
