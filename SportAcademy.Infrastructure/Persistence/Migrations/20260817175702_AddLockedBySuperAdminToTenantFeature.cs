using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLockedBySuperAdminToTenantFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LockedBySuperAdmin",
                table: "TenantFeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedBySuperAdmin",
                table: "TenantFeatures");
        }
    }
}
