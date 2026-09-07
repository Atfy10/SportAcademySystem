using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    // Backfills every tenant that already existed before this migration to IsSetupComplete =
    // true - the post-invite "complete your academy profile" onboarding wizard this flag gates
    // is new, and an existing tenant's Owner should never be retroactively interrupted by a
    // step that didn't exist when they onboarded. Only a tenant created after this migration
    // starts at the column's real default (false) and actually sees the wizard.
    /// <inheritdoc />
    public partial class AddPersonImageAndTenantSetupFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Trainees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSetupComplete",
                table: "TenantProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Employees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("UPDATE [TenantProfiles] SET [IsSetupComplete] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Trainees");

            migrationBuilder.DropColumn(
                name: "IsSetupComplete",
                table: "TenantProfiles");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Employees");
        }
    }
}
