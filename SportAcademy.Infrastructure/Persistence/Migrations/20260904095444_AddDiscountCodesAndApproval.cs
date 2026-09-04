using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCodesAndApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountCodeId",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscountCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PercentageOff = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExpiresAt = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountCodes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionDiscountRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraineeId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionTypeId = table.Column<int>(type: "int", nullable: false),
                    SportId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    DiscountCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedSubscriptionDetailsId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionDiscountRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_Sports_SportId",
                        column: x => x.SportId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_SubscriptionDetails_CreatedSubscriptionDetailsId",
                        column: x => x.CreatedSubscriptionDetailsId,
                        principalTable: "SubscriptionDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_SubscriptionTypes_SubscriptionTypeId",
                        column: x => x.SubscriptionTypeId,
                        principalTable: "SubscriptionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionDiscountRequests_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_DiscountCodeId",
                table: "InvoiceLines",
                column: "DiscountCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCodes_TenantId_Code",
                table: "DiscountCodes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_BranchId",
                table: "SubscriptionDiscountRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_CreatedSubscriptionDetailsId",
                table: "SubscriptionDiscountRequests",
                column: "CreatedSubscriptionDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_PaymentTypeId",
                table: "SubscriptionDiscountRequests",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_SportId",
                table: "SubscriptionDiscountRequests",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_SubscriptionTypeId",
                table: "SubscriptionDiscountRequests",
                column: "SubscriptionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_TenantId",
                table: "SubscriptionDiscountRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDiscountRequests_TraineeId",
                table: "SubscriptionDiscountRequests",
                column: "TraineeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_DiscountCodes_DiscountCodeId",
                table: "InvoiceLines",
                column: "DiscountCodeId",
                principalTable: "DiscountCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_DiscountCodes_DiscountCodeId",
                table: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "DiscountCodes");

            migrationBuilder.DropTable(
                name: "SubscriptionDiscountRequests");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_DiscountCodeId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "DiscountCodeId",
                table: "InvoiceLines");
        }
    }
}
