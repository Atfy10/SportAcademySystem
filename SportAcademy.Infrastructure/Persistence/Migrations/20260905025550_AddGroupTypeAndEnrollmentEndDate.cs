using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupTypeAndEnrollmentEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SportPrices",
                table: "SportPrices");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SubscriptionDiscountRequests");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TraineeGroups",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "SubscriptionDiscountRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "TrainingDays",
                table: "SubscriptionDiscountRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "SubscriptionDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "TrainingDays",
                table: "SubscriptionDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "SportPrices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SportPrices",
                table: "SportPrices",
                columns: new[] { "SportId", "BranchId", "SubsTypeId", "GroupType" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId_GroupType",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId", "GroupType" });

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId_GroupType",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId", "GroupType" },
                principalTable: "SportPrices",
                principalColumns: new[] { "SportId", "BranchId", "SubsTypeId", "GroupType" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId_GroupType",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId_GroupType",
                table: "SubscriptionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SportPrices",
                table: "SportPrices");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TraineeGroups");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "SubscriptionDiscountRequests");

            migrationBuilder.DropColumn(
                name: "TrainingDays",
                table: "SubscriptionDiscountRequests");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "TrainingDays",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "SportPrices");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Enrollments");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "SubscriptionDiscountRequests",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SportPrices",
                table: "SportPrices",
                columns: new[] { "SportId", "BranchId", "SubsTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_SportPrices_SportId_BranchId_SubscriptionTypeId",
                table: "SubscriptionDetails",
                columns: new[] { "SportId", "BranchId", "SubscriptionTypeId" },
                principalTable: "SportPrices",
                principalColumns: new[] { "SportId", "BranchId", "SubsTypeId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
