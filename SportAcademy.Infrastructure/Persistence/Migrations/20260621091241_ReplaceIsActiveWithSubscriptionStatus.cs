using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsActiveWithSubscriptionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SubscriptionDetails");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SubscriptionDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SubscriptionDetails");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SubscriptionDetails",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
