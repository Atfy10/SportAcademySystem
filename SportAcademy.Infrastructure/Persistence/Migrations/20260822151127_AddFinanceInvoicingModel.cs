using System;
using Microsoft.EntityFrameworkCore.Migrations;
using SportAcademy.Infrastructure.Persistence.Sql;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // Replaces the 1:1 Payment<->SubscriptionDetails relationship (which could not express a
    // partial payment, an instalment, or one payment covering several invoices) with the
    // standard accounts-receivable core: Invoice (what's owed) -> Payment (what's received) ->
    // PaymentAllocation (which invoice a payment settles, and how much of it). See
    // Domain.Entities.Finance for the model and IFinanceLedgerService for the only code allowed
    // to mutate it.
    //
    // Every existing SubscriptionDetails row already has exactly one Payment (the old FK was
    // required), so the backfill below is exact: one Invoice + one InvoiceLine per subscription,
    // fully paid, linked back to the subscription's existing Payment via one PaymentAllocation.
    // Nothing is lost, but PaymentNumber leaving SubscriptionDetails is irreversible in the sense
    // that Down() recreates the column without repopulating it - take a database backup before
    // applying this to an environment with real data.
    public partial class AddFinanceInvoicingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. New Payment columns first (nullable-safe defaults) - backfilled below before
            //    the old PaymentNumber link on SubscriptionDetails is dropped.
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "KWD");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedByUserId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Payments",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Completed");

            // 2. New tables.
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TraineeId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Trainees_TraineeId",
                        column: x => x.TraineeId,
                        principalTable: "Trainees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SubscriptionDetailsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_SubscriptionDetails_SubscriptionDetailsId",
                        column: x => x.SubscriptionDetailsId,
                        principalTable: "SubscriptionDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Payments_PaymentNumber",
                        column: x => x.PaymentNumber,
                        principalTable: "Payments",
                        principalColumn: "PaymentNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_SubscriptionDetailsId",
                table: "InvoiceLines",
                column: "SubscriptionDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BranchId",
                table: "Invoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TraineeId",
                table: "Invoices",
                column: "TraineeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_InvoiceId",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentNumber",
                table: "PaymentAllocations",
                column: "PaymentNumber");

            // 3. Backfill: one Invoice + one InvoiceLine per existing SubscriptionDetails row,
            //    priced from the SportPrice join (the same source the old code read the price
            //    from live - see Finance.InvoiceLine remarks on why that price is now
            //    snapshotted instead), fully paid, linked to that subscription's existing
            //    Payment via one PaymentAllocation. The old schema required every
            //    SubscriptionDetails to have exactly one Payment, so this is exact - no
            //    orphaned or partial rows possible here.
            migrationBuilder.Sql(@"
                INSERT INTO [Invoices]
                    ([InvoiceNumber], [Status], [IssueDate], [DueDate], [TraineeId], [BranchId],
                     [Currency], [SubTotal], [DiscountTotal], [TaxTotal], [GrandTotal], [AmountPaid],
                     [IsDeleted], [CreatedAt], [TenantId])
                SELECT
                    CONCAT(N'INV-', YEAR(sd.[StartDate]), N'-', RIGHT(N'00000' + CAST(sd.[Id] AS VARCHAR(5)), 5)),
                    N'Paid',
                    sd.[StartDate],
                    sd.[StartDate],
                    sd.[TraineeId],
                    sd.[BranchId],
                    N'KWD',
                    sp.[Price],
                    0,
                    0,
                    sp.[Price],
                    sp.[Price],
                    sd.[IsDeleted],
                    sd.[CreatedAt],
                    sd.[TenantId]
                FROM [SubscriptionDetails] sd
                INNER JOIN [SportPrices] sp
                    ON sp.[SportId] = sd.[SportId] AND sp.[BranchId] = sd.[BranchId] AND sp.[SubsTypeId] = sd.[SubscriptionTypeId];

                INSERT INTO [InvoiceLines]
                    ([InvoiceId], [Type], [Description], [Quantity], [UnitPrice], [DiscountAmount], [LineTotal], [SubscriptionDetailsId])
                SELECT
                    i.[Id],
                    N'SubscriptionFee',
                    N'Subscription fee',
                    1,
                    i.[GrandTotal],
                    0,
                    i.[GrandTotal],
                    sd.[Id]
                FROM [SubscriptionDetails] sd
                INNER JOIN [Invoices] i
                    ON i.[TraineeId] = sd.[TraineeId] AND i.[BranchId] = sd.[BranchId] AND i.[IssueDate] = sd.[StartDate]
                    AND i.[InvoiceNumber] = CONCAT(N'INV-', YEAR(sd.[StartDate]), N'-', RIGHT(N'00000' + CAST(sd.[Id] AS VARCHAR(5)), 5));

                INSERT INTO [PaymentAllocations] ([PaymentNumber], [InvoiceId], [Amount])
                SELECT sd.[PaymentNumber], i.[Id], i.[GrandTotal]
                FROM [SubscriptionDetails] sd
                INNER JOIN [InvoiceLines] il ON il.[SubscriptionDetailsId] = sd.[Id]
                INNER JOIN [Invoices] i ON i.[Id] = il.[InvoiceId];

                UPDATE p
                SET p.[Amount] = i.[GrandTotal]
                FROM [Payments] p
                INNER JOIN [PaymentAllocations] pa ON pa.[PaymentNumber] = p.[PaymentNumber]
                INNER JOIN [Invoices] i ON i.[Id] = pa.[InvoiceId];
            ");

            // 4. Now safe to drop the old link - every subscription's payment has been
            //    captured above.
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionDetails_Payments_PaymentNumber",
                table: "SubscriptionDetails");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionDetails_PaymentNumber",
                table: "SubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "SubscriptionDetails");

            // 5. Tenant-scoped, per-year, gap-free document numbering (see
            //    SqlFinancialDocumentNumberGenerator) - same mechanism as
            //    usp_GenerateTraineeCode, just keyed by (TenantId, DocumentType, Year) instead
            //    of (FamilyId).
            migrationBuilder.CreateTable(
                name: "DocumentNumberCounters",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberCounters", x => new { x.TenantId, x.DocumentType, x.Year });
                });

            // 5b. The backfilled invoices above were numbered directly from
            //     SubscriptionDetails.Id (e.g. "INV-2026-00001"), not through
            //     usp_GenerateDocumentNumber, so DocumentNumberCounters starts this migration
            //     empty regardless of how many invoices were just backfilled. Without this, the
            //     first real invoice issued after the migration re-generates "INV-2026-00001"
            //     and collides with the backfilled row of the same number. Seed each tenant's
            //     counter from the highest number actually present in its backfilled invoices.
            migrationBuilder.Sql(@"
                ;WITH InvoiceNumbers AS (
                    SELECT
                        [TenantId],
                        CAST(SUBSTRING([InvoiceNumber], 5, 4) AS INT) AS [Year],
                        CAST(RIGHT([InvoiceNumber], 5) AS INT) AS [Number]
                    FROM [Invoices]
                    WHERE [InvoiceNumber] LIKE N'INV-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9]'
                ),
                MaxPerTenantYear AS (
                    SELECT [TenantId], [Year], MAX([Number]) AS [MaxNumber]
                    FROM InvoiceNumbers
                    GROUP BY [TenantId], [Year]
                )
                MERGE [DocumentNumberCounters] AS target
                USING (SELECT [TenantId], N'INV' AS [DocumentType], [Year], [MaxNumber] FROM MaxPerTenantYear) AS src
                    ON target.[TenantId] = src.[TenantId]
                       AND target.[DocumentType] = src.[DocumentType]
                       AND target.[Year] = src.[Year]
                WHEN MATCHED AND target.[LastNumber] < src.[MaxNumber] THEN
                    UPDATE SET [LastNumber] = src.[MaxNumber]
                WHEN NOT MATCHED THEN
                    INSERT ([TenantId], [DocumentType], [Year], [LastNumber])
                    VALUES (src.[TenantId], src.[DocumentType], src.[Year], src.[MaxNumber]);
            ");

            migrationBuilder.Sql(
                SqlFileReader.Read("Procedures/usp_GenerateDocumentNumber.sql"),
                suppressTransaction: true);

            // 6. vw_TraineeSubscription selected SubscriptionDetails.PaymentNumber directly -
            //    re-apply it now that the column is gone. CREATE OR ALTER is idempotent, so
            //    re-running the (now-corrected) file is enough; no separate ALTER VIEW needed.
            migrationBuilder.Sql(SqlFileReader.Read("Views/vw_TraineeSubscription.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.usp_GenerateDocumentNumber', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.usp_GenerateDocumentNumber;
            ");

            migrationBuilder.DropTable(
                name: "DocumentNumberCounters");

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "SubscriptionDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Best-effort restore from the backfilled allocations - exact for any database that
            // only ever had this migration's Up() applied and nothing since (a payment settling
            // more than one invoice, which only becomes possible after Up(), cannot be
            // represented back on the old 1:1 column).
            migrationBuilder.Sql(@"
                UPDATE sd
                SET sd.[PaymentNumber] = pa.[PaymentNumber]
                FROM [SubscriptionDetails] sd
                INNER JOIN [InvoiceLines] il ON il.[SubscriptionDetailsId] = sd.[Id]
                INNER JOIN [PaymentAllocations] pa ON pa.[InvoiceId] = il.[InvoiceId];
            ");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_PaymentNumber",
                table: "SubscriptionDetails",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionDetails_Payments_PaymentNumber",
                table: "SubscriptionDetails",
                column: "PaymentNumber",
                principalTable: "Payments",
                principalColumn: "PaymentNumber",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RecordedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");
        }
    }
}
