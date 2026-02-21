using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EnrollmentRepository : BaseRepository<Enrollment, int>, IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EnrollmentsSportDto> GetAllEnrollmentsForSport(
            PageRequest page,
            DateTime? from,
            DateTime? to,
            int sportId,
            CancellationToken ct = default)
        {
            var baseQuery = _context.Enrollments
                .Where(e => e.TraineeGroup.Coach.SportId == sportId)
                .Where(e => e.EnrollmentDate >= (from ?? DateTime.UtcNow.AddDays(-30))
                         && e.EnrollmentDate < (to ?? DateTime.UtcNow));

            var totalCount = await baseQuery.CountAsync(ct);

            var enrollments = await baseQuery
                .Select(e => new EnrollmentDataDto(
                    e.Id,
                    e.EnrollmentDate,
                    e.ExpiryDate,
                    e.SessionAllowed,
                    e.SessionRemaining,
                    e.IsActive,
                    e.Trainee.FirstName,
                    e.TraineeGroup.Coach.Employee.FirstName,
                    e.SubscriptionDetailsId
                ))
                .AsNoTracking()
                .ToListAsync(ct);

            var pagedEnrollments = enrollments.ToPagedData(page);

            var sportName = await _context.Sports
                .Where(s => s.Id == sportId)
                .Select(s => s.Name)
                .FirstAsync(ct);

            var result = new EnrollmentsSportDto(
                SportName: sportName,
                Enrollments: pagedEnrollments
            );

            return result;
        }

        public async Task<int> GetEnrollmentsCountForSport(
            int sportId,
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default)
            => await _context.Enrollments
                .Where(e => e.TraineeGroup.Coach.SportId == sportId)
                .Where(e => e.EnrollmentDate >= (from ?? DateTime.UtcNow.AddDays(-30))
                    && e.EnrollmentDate < (to ?? DateTime.UtcNow))
                .CountAsync(ct);

        public async Task<int> GetEnrollmentsCountForSports(
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default)
            => await _context.Enrollments
                .Where(e => e.EnrollmentDate >= (from ?? DateTime.UtcNow.AddDays(-30))
                    && e.EnrollmentDate < (to ?? DateTime.UtcNow))
                .CountAsync(ct);

        public async Task<PagedData<EnrollmentsSportsDto>> GetAllEnrollmentsForAllSports(
            PageRequest page,
            DateTime? from,
            DateTime? to,
            CancellationToken ct)
        {
            from ??= DateTime.UtcNow.AddDays(-30);
            to ??= DateTime.UtcNow;

            var sportsPage = await _context.Enrollments
                .Where(e => e.EnrollmentDate >= from && e.EnrollmentDate < to)
                .Select(e => e.TraineeGroup.Coach.Sport.Name)
                .Distinct()
                .OrderBy(s => s)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(ct);

            var enrollments = await _context.Enrollments
                .Where(e => sportsPage.Contains(e.TraineeGroup.Coach.Sport.Name))
                .Where(e => e.EnrollmentDate >= from && e.EnrollmentDate < to)
                .Select(e => new
                {
                    SportName = e.TraineeGroup.Coach.Sport.Name,
                    Enrollment = new EnrollmentDataDto(
                        e.Id,
                        e.EnrollmentDate,
                        e.ExpiryDate,
                        e.SessionAllowed,
                        e.SessionRemaining,
                        e.IsActive,
                        e.Trainee.FirstName,
                        e.TraineeGroup.Coach.Employee.FirstName,
                        e.SubscriptionDetailsId
                    )
                })
                .ToListAsync(ct);

            var grouped = enrollments
                .GroupBy(x => x.SportName)
                .Select(g => new EnrollmentsSportsDto(
                    g.Select(x => x.Enrollment).ToList(),
                    g.Key
                ))
                .ToList();

            return grouped.ToPagedData(page);
        }
    }
}
