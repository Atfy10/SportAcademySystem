using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class buildBaseOfSemanticTraineeCode_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NationalityCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees",
                column: "FamilyId",
                filter: "[FamilyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "NationalityCategories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees",
                column: "FamilyId");
        }
    }
}
