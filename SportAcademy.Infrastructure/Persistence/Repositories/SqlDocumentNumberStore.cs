using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    // See IDocumentNumberStore for why "is this number taken?" has to look across every tenant.
    // All raw SQL here deliberately bypasses EF's tenant/soft-delete query filters: the target
    // keys (Payments.PaymentNumber, Invoices.InvoiceNumber) are global, so a row that is hidden
    // from the current tenant still blocks the insert.
    public class SqlDocumentNumberStore : IDocumentNumberStore
    {
        // Where each document type's numbers actually live. Identifiers come from this constant
        // table only, never from input, so interpolating them into the SQL below is safe.
        private static readonly Dictionary<string, (string Table, string Column)> Targets = new()
        {
            ["PAY"] = ("Payments", "PaymentNumber"),
            ["INV"] = ("Invoices", "InvoiceNumber"),
        };

        private readonly ApplicationDbContext _context;
        private readonly ITenantIdProvider _tenantIdProvider;

        public SqlDocumentNumberStore(ApplicationDbContext context, ITenantIdProvider tenantIdProvider)
        {
            _context = context;
            _tenantIdProvider = tenantIdProvider;
        }

        private Guid RequireTenantId() =>
            _tenantIdProvider.TenantId
            ?? throw new InvalidOperationException("Cannot generate a document number without an ambient tenant.");

        public async Task<string> NextFromTenantCounterAsync(string documentType, CancellationToken ct = default)
        {
            var result = new SqlParameter
            {
                ParameterName = "@DocumentNumber",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 50,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_GenerateDocumentNumber @TenantId, @DocumentType, @Year, @DocumentNumber OUTPUT",
                new SqlParameter("@TenantId", RequireTenantId()),
                new SqlParameter("@DocumentType", documentType),
                new SqlParameter("@Year", DateTime.UtcNow.Year),
                result
            );

            return result.Value!.ToString()!;
        }

#pragma warning disable EF1002 // identifiers come from the constant Targets table, values are parameters
        public async Task<bool> ExistsAsync(string documentType, string number, CancellationToken ct = default)
        {
            if (!Targets.TryGetValue(documentType, out var target))
                return false;

            var rows = await _context.Database
                .SqlQueryRaw<int>(
                    $"SELECT CASE WHEN EXISTS (SELECT 1 FROM [{target.Table}] WHERE [{target.Column}] = @number) THEN 1 ELSE 0 END AS [Value]",
                    new SqlParameter("@number", number))
                .ToListAsync(ct);

            return rows.Single() == 1;
        }

        public async Task<int> HighestUsedAsync(string documentType, int year, CancellationToken ct = default)
        {
            if (!Targets.TryGetValue(documentType, out var target))
                return 0;

            var rows = await _context.Database
                .SqlQueryRaw<int>(
                    $"SELECT ISNULL(MAX(TRY_CAST(RIGHT([{target.Column}], 5) AS INT)), 0) AS [Value] " +
                    $"FROM [{target.Table}] WHERE [{target.Column}] LIKE @pattern",
                    new SqlParameter("@pattern", $"{documentType}-{year}-[0-9][0-9][0-9][0-9][0-9]"))
                .ToListAsync(ct);

            return rows.Single();
        }
#pragma warning restore EF1002

        public Task RaiseTenantCounterAsync(string documentType, int year, int atLeast, CancellationToken ct = default) =>
            _context.Database.ExecuteSqlRawAsync(@"
                MERGE DocumentNumberCounters AS target
                USING (SELECT @TenantId AS TenantId, @DocumentType AS DocumentType, @Year AS [Year]) AS src
                    ON target.TenantId = src.TenantId
                       AND target.DocumentType = src.DocumentType
                       AND target.[Year] = src.[Year]
                WHEN MATCHED AND target.LastNumber < @AtLeast THEN
                    UPDATE SET LastNumber = @AtLeast
                WHEN NOT MATCHED THEN
                    INSERT (TenantId, DocumentType, [Year], LastNumber)
                    VALUES (src.TenantId, src.DocumentType, src.[Year], @AtLeast);",
                new object[]
                {
                    new SqlParameter("@TenantId", RequireTenantId()),
                    new SqlParameter("@DocumentType", documentType),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@AtLeast", atLeast),
                },
                ct);
    }
}
