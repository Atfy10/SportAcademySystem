using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class buildBaseOfSemanticTraineeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FamilyId",
                table: "Trainees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityCategoryId",
                table: "Trainees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraineeCode",
                table: "Trainees",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyCode = table.Column<int>(type: "int", nullable: false),
                    LastMemberNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NationalityCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalityCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NationalityCategories",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "AM", "American" },
                    { 2, "EU", "European" },
                    { 3, "AS", "Asian" },
                    { 4, "AF", "African" },
                    { 5, "AG", "Arab Gulf" },
                    { 6, "AR", "Arab" },
                    { 7, "OC", "Oceanian" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_NationalityCategoryId",
                table: "Trainees",
                column: "NationalityCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_TraineeCode",
                table: "Trainees",
                column: "TraineeCode",
                unique: true,
                filter: "[TraineeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NationalityCategories_Code",
                table: "NationalityCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NationalityCategories_Name",
                table: "NationalityCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainees_NationalityCategories_NationalityCategoryId",
                table: "Trainees",
                column: "NationalityCategoryId",
                principalTable: "NationalityCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_NationalityCategories_NationalityCategoryId",
                table: "Trainees");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropTable(
                name: "NationalityCategories");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_NationalityCategoryId",
                table: "Trainees");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_TraineeCode",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "NationalityCategoryId",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "TraineeCode",
                table: "Trainees");
        }
    }
}
