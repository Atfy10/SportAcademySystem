using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces;

public interface ISessionOccurrenceRepository : IBaseRepository<SessionOccurrence, int>
{
    Task<PagedData<SessionOccurrenceDto>> GetAllPaginatedAsync(PageRequest page, CancellationToken cancellationToken = default);
    Task<PagedData<SessionOccurrenceDto>> GetByDateAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default);
    Task<PagedData<SessionOccurrenceDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default);
    Task<int?> GetTraineeGroupIdAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default);
    /// <summary>The session's TraineeGroupId, StartDateTime and duration - used to enforce the
    /// "no marking attendance more than 15 minutes after a session ends" cutoff.</summary>
    Task<(int TraineeGroupId, DateTime StartDateTime, int DurationInMinutes)?> GetTimingAsync(
        int sessionOccurrenceId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastOccurrenceDateAsync(int traineeGroupId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<SessionOccurrence> entities, CancellationToken cancellationToken = default);

    /// <summary>Flips every future (StartDateTime &gt; asOf) session of traineeGroupId currently at
    /// fromStatus to toStatus. Used to temporarily cancel a paused group's upcoming sessions and to
    /// restore them on resume. Returns the number of sessions changed.</summary>
    Task<int> SetFutureSessionsStatusAsync(
        int traineeGroupId, SessionStatus fromStatus, SessionStatus toStatus, DateTime asOf, CancellationToken cancellationToken = default);

    /// <summary>Occurrences of the same recurring weekly slot (GroupSchedule) as sessionOccurrenceId,
    /// ordered chronologically: up to pastCount before it, itself, then up to futureCount after it.
    /// Empty if sessionOccurrenceId doesn't exist.</summary>
    Task<List<SessionOccurrenceDto>> GetNearbyOccurrencesAsync(
        int sessionOccurrenceId, int pastCount, int futureCount, CancellationToken cancellationToken = default);
}
