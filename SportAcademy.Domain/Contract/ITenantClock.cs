namespace SportAcademy.Domain.Contract
{
    /// <summary>
    /// Resolves "now" in the current tenant's configured timezone, not the server's/UTC's.
    /// </summary>
    /// <remarks>
    /// Day-boundary business logic (which calendar day a newly generated session lands on,
    /// which day an attendance record is for, whether a check-in window has closed) has to agree
    /// with what the tenant's own staff consider "today" - a raw DateTime.UtcNow rolls over at
    /// the wrong wall-clock moment for every tenant not in UTC (e.g. a Kuwait, UTC+3, coach
    /// working past 9pm local is still before UTC midnight, but one working before 3am local is
    /// already past it). Session/schedule times themselves are written and compared as tenant
    /// wall-clock values throughout this codebase (no UTC conversion happens when they're
    /// created), so "now" has to be resolved the same way to compare correctly against them.
    /// </remarks>
    public interface ITenantClock
    {
        /// <summary>Current wall-clock time in the tenant's configured timezone, or plain UTC
        /// now if there is no tenant context or no timezone configured.</summary>
        Task<DateTime> GetLocalNowAsync(CancellationToken cancellationToken = default);
    }
}
