using SportAcademy.Application.Common.Pagination;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // From/To are inclusive calendar days matched against the event's own span (an event
    // counts if any part of it falls in the range). Status is resolved against Today with the
    // same rules as EventStatusRules. Term matches the title, the customer's name or phone.
    public record EventListFilter(
        int? BranchId = null,
        DateOnly? From = null,
        DateOnly? To = null,
        EventStatus? Status = null,
        int? CustomerId = null,
        Guid? CreatedByUserId = null,
        string? Term = null);

    public interface IEventRepository : IBaseRepository<Event, int>
    {
        // Read-only, with everything EventMapper needs (branch/nationality translations, invoice).
        Task<(List<Event> Items, int TotalCount)> GetPagedAsync(
            EventListFilter filter, DateOnly today, PageRequest page, CancellationToken ct = default);

        Task<List<Event>> GetForReportAsync(
            EventListFilter filter, DateOnly today, int maxRows, CancellationToken ct = default);

        // Tracked when forUpdate is true (update/cancel/delete), with the invoice's lines and
        // allocations (+ their payments) loaded.
        Task<Event?> GetWithDetailsAsync(int id, bool forUpdate, CancellationToken ct = default);

        // Non-cancelled events at the branch whose time span intersects [startsAt, endsAt).
        Task<List<Event>> GetOverlappingAsync(
            int branchId, DateTime startsAt, DateTime endsAt, int? excludeId, CancellationToken ct = default);
    }
}
