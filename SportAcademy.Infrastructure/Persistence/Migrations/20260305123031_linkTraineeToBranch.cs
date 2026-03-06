using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class linkTraineeToBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Trainees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_BranchId",
                table: "Trainees",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainees_Branches_BranchId",
                table: "Trainees",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_Branches_BranchId",
                table: "Trainees");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_BranchId",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Trainees");
        }
    }
}
