using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.ExcuseRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ExcuseRequestRepository : BaseRepository<ExcuseRequest, int>, IExcuseRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ExcuseRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsPendingAsync(int sessionOccurrenceId, int traineeId, CancellationToken cancellationToken = default)
            => await _context.ExcuseRequests.AnyAsync(
                x => x.SessionOccurrenceId == sessionOccurrenceId
                    && x.TraineeId == traineeId
                    && x.Status == ExcuseRequestStatus.Pending,
                cancellationToken);

        public async Task<int> CountPendingAsync(CancellationToken cancellationToken = default)
            => await _context.ExcuseRequests.CountAsync(x => x.Status == ExcuseRequestStatus.Pending, cancellationToken);

        public async Task<PagedData<ExcuseRequestDto>> GetPendingAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.ExcuseRequests
                .Where(x => x.Status == ExcuseRequestStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .AsNoTracking()
                .Select(x => new ExcuseRequestDto(
                    x.Id,
                    x.SessionOccurrenceId,
                    DateOnly.FromDateTime(x.SessionOccurrence.StartDateTime),
                    x.SessionOccurrence.StartDateTime.ToString("HH:mm:ss"),
                    x.TraineeId,
                    x.Trainee.FirstName + " " + x.Trainee.LastName,
                    x.SessionOccurrence.GroupSchedule.TraineeGroup.Name,
                    x.SessionOccurrence.GroupSchedule.TraineeGroup.Coach.Sport.Name,
                    x.Reason,
                    x.Status.ToString(),
                    x.CreatedAt,
                    x.ReviewedAt,
                    x.ReviewNote
                ))
                .ToPagedDataAsync(page, cancellationToken);
    }
}
