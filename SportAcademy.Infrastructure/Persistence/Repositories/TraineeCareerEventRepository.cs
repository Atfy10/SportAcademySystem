using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeCareerEventRepository : BaseRepository<TraineeCareerEvent, int>, ITraineeCareerEventRepository
    {
        private readonly ApplicationDbContext _context;

        public TraineeCareerEventRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<TraineeCareerEvent>> GetSkillEventsForTraineeAsync(int traineeId, CancellationToken ct = default)
            => await _context.TraineeCareerEvents
                .Where(e => e.TraineeId == traineeId && e.EventType == TraineeCareerEventType.SkillLevelChanged)
                .Include(e => e.Sport)
                .OrderBy(e => e.EffectiveDate)
                .ToListAsync(ct);

        public async Task<List<TraineeCareerEvent>> GetCoachEventsForTraineeAsync(int traineeId, CancellationToken ct = default)
        {
            var events = await _context.TraineeCareerEvents
                .Where(e => e.TraineeId == traineeId && e.EventType == TraineeCareerEventType.CoachAssigned)
                .Include(e => e.Sport)
                .Include(e => e.Coach!).ThenInclude(c => c.Employee)
                .Include(e => e.TraineeGroup)
                .OrderBy(e => e.EffectiveDate)
                .ToListAsync(ct);

            // Enrollment is deliberately NOT Include()'d above: the global soft-delete query
            // filter applies to Included navigations too, so a soft-deleted enrollment would
            // silently load as null here - and the caller (GetTraineeCoachHistoryQueryHandler)
            // specifically needs to see IsDeleted:true enrollments to close out a coaching
            // stint at its deletion date. Load them separately with IgnoreQueryFilters(),
            // scoped back to the current tenant by hand since that filter also carries the
            // tenant check.
            var enrollmentIds = events
                .Where(e => e.EnrollmentId.HasValue)
                .Select(e => e.EnrollmentId!.Value)
                .Distinct()
                .ToList();

            if (enrollmentIds.Count > 0)
            {
                var enrollments = await _context.Enrollments
                    .IgnoreQueryFilters()
                    .Where(en => en.TenantId == _context.CurrentTenantId && enrollmentIds.Contains(en.Id))
                    .ToDictionaryAsync(en => en.Id, ct);

                foreach (var e in events)
                {
                    if (e.EnrollmentId.HasValue && enrollments.TryGetValue(e.EnrollmentId.Value, out var enrollment))
                        e.Enrollment = enrollment;
                }
            }

            return events;
        }
    }
}
