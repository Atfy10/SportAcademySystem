using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class makeFamilyCodeSequenceAsDefaultForFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Trainees");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS dbo.FamilyCodeSequence;");

            migrationBuilder.CreateSequence<int>(
                name: "FamilyCodeSequence",
                startValue: 1L,
                incrementBy: 1);

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false,
                        defaultValueSql: "NEXT VALUE FOR dbo.FamilyCodeSequence"),

                    FamilyCode = table.Column<int>(
                        type: "int",
                        nullable: false),

                    LastMemberNumber = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "FamilyId",
                table: "Trainees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees");

            migrationBuilder.DropIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Trainees");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropSequence(
                name: "FamilyCodeSequence");

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    FamilyCode = table.Column<int>(
                        type: "int",
                        nullable: false),

                    LastMemberNumber = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "FamilyId",
                table: "Trainees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainees_FamilyId",
                table: "Trainees",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainees_Families_FamilyId",
                table: "Trainees",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");
        }
    }
}
