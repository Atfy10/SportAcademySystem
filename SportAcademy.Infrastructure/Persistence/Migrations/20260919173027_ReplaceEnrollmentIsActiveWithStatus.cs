using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEnrollmentIsActiveWithStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Status first (every row starts 'Active' via the default) and backfill it from
            // the still-present IsActive/EndDate columns before dropping IsActive - the scaffolded
            // order (drop then add) would have silently lost which enrollments were suspended.
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql(@"
                UPDATE [Enrollments]
                SET [Status] = CASE
                    WHEN [EndDate] IS NOT NULL THEN N'Ended'
                    WHEN [IsActive] = 0 THEN N'Suspended'
                    ELSE N'Active'
                END;
            ");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Enrollments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE [Enrollments]
                SET [IsActive] = CASE WHEN [Status] = N'Active' THEN 1 ELSE 0 END;
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");
        }
    }
}
