using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addTraineeCodesHistoryTableToSchemaAndCreateFamilyCodeSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraineeCodesHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    OldTraineeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeCodesHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeCodesHistory_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCodesHistory_TraineeId",
                table: "TraineeCodesHistory",
                column: "TraineeId");

            migrationBuilder.Sql(@"
                CREATE SEQUENCE FamilyCodeSequence
                    AS INT
                    START WITH 1
                    INCREMENT BY 1
                    MINVALUE 1
                    NO CYCLE
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraineeCodesHistory");

            migrationBuilder.Sql(@"
                DROP SEQUENCE IF EXISTS FamilyCodeSequence
            ");
        }
    }
}
