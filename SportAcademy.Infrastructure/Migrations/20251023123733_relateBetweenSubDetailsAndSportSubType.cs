using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class relateBetweenSubDetailsAndSportSubType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SubscriptionTypes_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.AddColumn<int>(
                name: "SportSubscriptionTypeSportId",
                table: "SubscriptionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SportSubscriptionTypeSportId_SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportSubscriptionTypeSportId", "SportSubscriptionTypeSubscriptionTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_SportSubscriptionTypes_SportSubscriptionTypeSportId_SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportSubscriptionTypeSportId", "SportSubscriptionTypeSubscriptionTypeId" },
                principalTable: "SportSubscriptionTypes",
                principalColumns: new[] { "SportId", "SubscriptionTypeId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SportSubscriptionTypes_SportSubscriptionTypeSportId_SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SubscriptionTypes_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SportSubscriptionTypeSportId_SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "SportSubscriptionTypeSportId",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "SportSubscriptionTypeSubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionTypeId",
                table: "SubscriptionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SubscriptionTypeId",
                table: "SubscriptionDetails",
                column: "SubscriptionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_SubscriptionTypes_SubscriptionTypeId",
                table: "SubscriptionDetails",
                column: "SubscriptionTypeId",
                principalTable: "SubscriptionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
