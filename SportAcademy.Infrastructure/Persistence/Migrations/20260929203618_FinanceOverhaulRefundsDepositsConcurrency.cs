using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinanceOverhaulRefundsDepositsConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BalanceDueDate",
                table: "SubscriptionDiscountRequests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "SubscriptionDiscountRequests",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "ReversedAmount",
                table: "PaymentAllocations",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueSoonNotifiedOn",
                table: "Invoices",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OverdueNotifiedOn",
                table: "Invoices",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invoices",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "PaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefundedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Payments_PaymentNumber",
                        column: x => x.PaymentNumber,
                        principalTable: "Payments",
                        principalColumn: "PaymentNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DueDate",
                table: "Invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymentNumber",
                table: "PaymentRefunds",
                column: "PaymentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_RefundedAt",
                table: "PaymentRefunds",
                column: "RefundedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_TenantId",
                table: "PaymentRefunds",
                column: "TenantId");

            // Backfill 1: how much of each allocation earlier refunds/voids already took back,
            // walked oldest allocation first (the order the ledger reverses in). Without it a
            // later refund could reverse the same money a second time.
            migrationBuilder.Sql(@"
;WITH Walk AS (
    SELECT pa.Id, pa.Amount, p.RefundedAmount,
           SUM(pa.Amount) OVER (PARTITION BY pa.PaymentNumber ORDER BY pa.Id ROWS UNBOUNDED PRECEDING) - pa.Amount AS Before
    FROM PaymentAllocations pa
    JOIN Payments p ON p.PaymentNumber = pa.PaymentNumber
    WHERE p.RefundedAmount > 0
)
UPDATE pa
SET ReversedAmount = CASE
        WHEN w.RefundedAmount - w.Before <= 0 THEN 0
        WHEN w.RefundedAmount - w.Before >= w.Amount THEN w.Amount
        ELSE w.RefundedAmount - w.Before
    END
FROM PaymentAllocations pa
JOIN Walk w ON w.Id = pa.Id;");

            // Backfill 2: one history row per payment already refunded/voided before refund
            // history existed, dated at the payment's last change - the best evidence of when.
            migrationBuilder.Sql(@"
INSERT INTO PaymentRefunds (PaymentNumber, Kind, Amount, Reason, RefundedAt, RefundedByUserId, CreatedAt, CreatedBy, TenantId)
SELECT p.PaymentNumber,
       CASE WHEN p.Status = 'Voided' THEN 'Void' ELSE 'Refund' END,
       p.RefundedAmount,
       N'Recorded before refund history was kept',
       COALESCE(p.UpdatedAt, p.PaidDate),
       NULL,
       COALESCE(p.UpdatedAt, p.PaidDate),
       p.UpdatedBy,
       p.TenantId
FROM Payments p
WHERE p.RefundedAmount > 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_DueDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BalanceDueDate",
                table: "SubscriptionDiscountRequests");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "SubscriptionDiscountRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReversedAmount",
                table: "PaymentAllocations");

            migrationBuilder.DropColumn(
                name: "DueSoonNotifiedOn",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OverdueNotifiedOn",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invoices");
        }
    }
}
