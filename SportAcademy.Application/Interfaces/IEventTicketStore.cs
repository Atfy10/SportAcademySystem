using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // What the guest's ticket page needs, found by the ticket's code before any tenant is known.
    public record PublicTicketTarget(
        Guid TenantId, TenantStatus TenantStatus, string AcademyName, string EventTitle, string BranchName,
        bool IsCancelled, DateTime StartsAtUtc, DateTime EndsAtUtc, int Number, string? GuestName, DateTime? AdmittedAt);

    public record EventTicketCounts(int Issued, int Admitted);

    public record CheckInEventRow(
        int Id, string Title, string BranchName, DateTime StartsAtUtc, DateTime EndsAtUtc, int Capacity, int Issued, int Admitted);

    // Event entry tickets. Everything except FindPublicAsync runs inside the caller's tenant (and
    // branch) scope through the normal query filters.
    public interface IEventTicketStore
    {
        // No tenant context: the code alone identifies the ticket (and so the academy).
        Task<PublicTicketTarget?> FindPublicAsync(string token, string lang, CancellationToken ct = default);

        // Read-only, with the event and its branch (and branch translations).
        Task<EventTicket?> FindByTokenAsync(string token, CancellationToken ct = default);
        Task<EventTicket?> FindByNumberAsync(int eventId, int number, CancellationToken ct = default);

        // Tracked, with its event - for rename / re-issue / revoke.
        Task<EventTicket?> GetForUpdateAsync(int ticketId, CancellationToken ct = default);

        Task<EventTicketCounts> GetCountsAsync(int eventId, CancellationToken ct = default);
        Task<List<int>> GetNumbersAsync(int eventId, CancellationToken ct = default);

        Task<(List<EventTicket> Items, int TotalCount)> GetPagedAsync(
            int eventId, EventTicketFilter filter, string? term, PageRequest page, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<EventTicket> tickets, CancellationToken ct = default);
        Task RemoveAsync(EventTicket ticket, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        // Marks the ticket used if nobody has yet. False when it was already used - including
        // when another doorman admitted it at the same moment.
        Task<bool> TryAdmitAsync(int ticketId, Guid admittedByUserId, DateTime nowUtc, CancellationToken ct = default);

        // Non-cancelled events still running or starting before untilUtc, with their ticket counts.
        Task<List<CheckInEventRow>> GetCheckInEventsAsync(DateTime nowUtc, DateTime untilUtc, string lang, CancellationToken ct = default);
    }
}
