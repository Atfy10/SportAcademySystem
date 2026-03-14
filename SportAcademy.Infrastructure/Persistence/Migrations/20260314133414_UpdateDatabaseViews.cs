using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var basePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "Sql",
                "Views"
            );

            var files = Directory.GetFiles(basePath, "*.sql");

            foreach (var file in files)
            {
                var sql = File.ReadAllText(file);
                migrationBuilder.Sql(sql);
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var basePath = Path.Combine(
                AppContext.BaseDirectory,
                "Persistence",
                "Sql",
                "Views"
            );

            var files = Directory.GetFiles(basePath, "*.sql");

            foreach (var file in files)
            {
                var viewName = Path.GetFileNameWithoutExtension(file);

                migrationBuilder.Sql($@"
                    DROP VIEW IF EXISTS dbo.{viewName};
                ");
            }

        }
    }
}
