using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class relateBetweenSportPriceAndSportSubTypeThenSportDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SportPrices_Sports_SportId",
                table: "SportPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SportPrices_SubscriptionTypes_SubsTypeId",
                table: "SportPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SubscriptionTypes_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SportPrices_SubsTypeId",
                table: "SportPrices");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "SubscriptionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "SubscriptionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SportPrices_SportId_SubsTypeId",
                table: "SportPrices",
                columns: new[] { "SportId", "SubsTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SportPrices_SportSubscriptionTypes_SportId_SubsTypeId",
                table: "SportPrices",
                columns: new[] { "SportId", "SubsTypeId" },
                principalTable: "SportSubscriptionTypes",
                principalColumns: new[] { "SportId", "SubscriptionTypeId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId" },
                principalTable: "SportPrices",
                principalColumns: new[] { "SportId", "BranchId", "SubsTypeId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SportPrices_SportSubscriptionTypes_SportId_SubsTypeId",
                table: "SportPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SportPrices_SportId_SubsTypeId",
                table: "SportPrices");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "SubscriptionDetails");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SubscriptionTypeId",
                table: "SubscriptionDetails",
                column: "SubscriptionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SportPrices_SubsTypeId",
                table: "SportPrices",
                column: "SubsTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SportPrices_Sports_SportId",
                table: "SportPrices",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportPrices_SubscriptionTypes_SubsTypeId",
                table: "SportPrices",
                column: "SubsTypeId",
                principalTable: "SubscriptionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
