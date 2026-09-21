namespace SportAcademy.Application.Interfaces
{
    /// <summary>
    /// Storage behind <see cref="IFinancialDocumentNumberGenerator"/>: the per-tenant counters, and
    /// a view of which document numbers are already taken.
    /// </summary>
    /// <remarks>
    /// The counters are per tenant (each tenant counts INV/PAY-YYYY-00001 from 1), but the numbers
    /// themselves are GLOBAL keys - Payments.PaymentNumber is the primary key and
    /// Invoices.InvoiceNumber has a unique index, neither of them scoped by tenant. So a tenant's
    /// counter can hand out a number that another tenant (or a row created some other way, e.g.
    /// seed data) already holds, and the insert then fails with a duplicate-key error. Everything
    /// here operates across ALL tenants precisely so the generator can see and avoid that.
    /// </remarks>
    public interface IDocumentNumberStore
    {
        /// <summary>The next number from the current tenant's atomic counter. It is not checked
        /// against anyone else's - see <see cref="ExistsAsync"/>.</summary>
        Task<string> NextFromTenantCounterAsync(string documentType, CancellationToken ct = default);

        /// <summary>True if any tenant already holds this exact number.</summary>
        Task<bool> ExistsAsync(string documentType, string number, CancellationToken ct = default);

        /// <summary>The highest sequence number any tenant has used for this type and year
        /// (0 when none).</summary>
        Task<int> HighestUsedAsync(string documentType, int year, CancellationToken ct = default);

        /// <summary>Raises the current tenant's counter to at least <paramref name="atLeast"/>.
        /// Never lowers it.</summary>
        Task RaiseTenantCounterAsync(string documentType, int year, int atLeast, CancellationToken ct = default);
    }
}
