using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    // What a scan needs about the event, found by its entry code before any tenant is known.
    public record EventEntryTarget(
        int EventId, Guid TenantId, TenantStatus TenantStatus, string AcademyName, string Title, string BranchName,
        bool IsCancelled, DateTime StartsAtUtc, DateTime EndsAtUtc, int Capacity, int AdmittedCount);

    public record AdmitOutcome(EventEntryResult Result, int AdmittedCount, EventAdmission? Admission);

    // The public entry-scan side of events. FindByTokenAsync works with no tenant context (the
    // scan is anonymous); every other method runs inside the event's own tenant (the caller
    // impersonates it first).
    public interface IEventEntryStore
    {
        Task<EventEntryTarget?> FindByTokenAsync(string token, string lang, CancellationToken ct = default);

        Task<EventAdmission?> FindAdmissionAsync(int eventId, string deviceKey, CancellationToken ct = default);

        // Lets one more person in if there is still room: Admitted (with the new admission),
        // Full, or AlreadyAdmitted if this device beat itself to it. Never lets the count pass
        // the capacity, even when several phones scan at the same moment.
        Task<AdmitOutcome> TryAdmitAsync(int eventId, string deviceKey, CancellationToken ct = default);
    }
}
