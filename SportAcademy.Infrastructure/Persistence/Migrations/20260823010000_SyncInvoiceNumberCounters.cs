using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncInvoiceNumberCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddFinanceInvoicingModel backfilled invoices numbered directly from
            // SubscriptionDetails.Id (e.g. "INV-2026-00001"), not through
            // usp_GenerateDocumentNumber, without seeding DocumentNumberCounters to match - so
            // on any database where that migration had already run before this fix landed, the
            // counter is still at 0 and the first real invoice issued re-generates
            // "INV-2026-00001", colliding with the backfilled row of the same number
            // (IX_Invoices_InvoiceNumber unique constraint violation). Idempotent and safe to
            // run on any database state, including a fresh one with zero invoices.
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data reconciliation only - no schema change, and reverting the counter to a
            // smaller value would reintroduce the exact collision this migration fixes.
        }
    }
}
