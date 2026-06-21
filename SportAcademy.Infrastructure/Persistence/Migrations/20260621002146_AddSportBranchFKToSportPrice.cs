using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportBranchFKToSportPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_SportPrices_SportBranches_SportId_BranchId",
                table: "SportPrices",
                columns: new[] { "SportId", "BranchId" },
                principalTable: "SportBranches",
                principalColumns: new[] { "SportId", "BranchId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SportPrices_SportBranches_SportId_BranchId",
                table: "SportPrices");
        }
    }
}
