using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Services
{
    /// <summary>
    /// Hands out <c>INV-2026-00001</c> / <c>PAY-2026-00001</c> style numbers that are guaranteed
    /// not to already exist.
    /// </summary>
    /// <remarks>
    /// The per-tenant counter alone is not enough: the numbers are global keys (see
    /// <see cref="IDocumentNumberStore"/>), so a counter that is behind - because another tenant
    /// holds the same number, or because rows were created without going through the counter -
    /// produces a number that is already taken, and creating the subscription/payment fails with a
    /// duplicate-key error. When that happens this jumps the counter past the highest number in
    /// use and takes the next one, so it heals itself instead of failing the request. The cost is
    /// that a tenant's numbers can skip values; they stay unique and increasing.
    /// </remarks>
    public class FinancialDocumentNumberGenerator : IFinancialDocumentNumberGenerator
    {
        // One jump past the highest used number normally resolves it; the remaining attempts only
        // cover another session taking the number in the instant between the check and the insert.
        public const int MaxAttempts = 5;

        private readonly IDocumentNumberStore _store;
        private readonly ILogger<FinancialDocumentNumberGenerator> _logger;

        public FinancialDocumentNumberGenerator(
            IDocumentNumberStore store,
            ILogger<FinancialDocumentNumberGenerator> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<string> GenerateAsync(string documentType, CancellationToken ct = default)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                var number = await _store.NextFromTenantCounterAsync(documentType, ct);

                if (!await _store.ExistsAsync(documentType, number, ct))
                    return number;

                var year = YearOf(number);
                var highest = await _store.HighestUsedAsync(documentType, year, ct);

                _logger.LogWarning(
                    "Document number {Number} is already taken by another row (attempt {Attempt}/{Max}); " +
                    "raising this tenant's {DocumentType}-{Year} counter to {Highest} and retrying.",
                    number, attempt, MaxAttempts, documentType, year, highest);

                await _store.RaiseTenantCounterAsync(documentType, year, highest, ct);
            }

            throw new InvalidOperationException(
                $"Could not generate a free {documentType} document number after {MaxAttempts} attempts.");
        }

        // "PAY-2026-00002" -> 2026
        private static int YearOf(string number)
        {
            var parts = number.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[1], out var year))
                return year;

            throw new InvalidOperationException($"Unexpected document number format: '{number}'.");
        }
    }
}
