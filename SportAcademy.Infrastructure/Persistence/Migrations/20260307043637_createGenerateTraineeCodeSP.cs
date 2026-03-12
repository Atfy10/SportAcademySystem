using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createGenerateTraineeCodeSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlFile = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "Sql",
                "Procedures",
                "usp_GenerateTraineeCode.sql");

            var sql = File.ReadAllText(sqlFile);

            migrationBuilder.Sql(sql, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_GenerateTraineeCode");
        }
    }
}
