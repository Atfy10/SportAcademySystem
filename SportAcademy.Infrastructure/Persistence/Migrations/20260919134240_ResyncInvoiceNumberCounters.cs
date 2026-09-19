using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResyncInvoiceNumberCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-runs SyncInvoiceNumberCounters' exact reconciliation (20260823010000): that
            // migration only patched tenants that already existed on 2026-08-23. Every tenant
            // seeded since then via SeedSalmiyaDataAsync gets its DocumentNumberCounters row
            // synced right after CreateSeedInvoices too - but that sync line was added to the
            // seeder after some tenants had already been seeded, so those tenants' counters are
            // still stuck at 0 while CreateSeedInvoices had already handed out
            // "INV-{year}-00001" directly. The first real invoice issued for one of those
            // tenants regenerates "INV-{year}-00001" and collides with the seed data's own row
            // (IX_Invoices_InvoiceNumber unique constraint violation) - this is the
            // "create subscription" failure users were hitting, since CreateSubscriptionDetails
            // issues an invoice as part of creation.
            //
            // Re-running the same MAX-per-tenant-year reconciliation catches any tenant
            // currently out of sync, regardless of when it was created - it only ever raises a
            // counter that's behind the real data (WHEN MATCHED AND target.LastNumber <
            // src.MaxNumber), never lowers one, so it's a no-op for every tenant already in sync
            // and safe to run on any database state, including a fresh one with zero invoices.
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
