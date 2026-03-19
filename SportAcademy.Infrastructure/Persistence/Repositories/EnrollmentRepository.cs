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

        public async Task<int?> GetEnrollmentIdAsync(int traineeId, int traineeGroupId, CancellationToken ct = default)
            => await _context.Enrollments
                .Where(e => e.TraineeId == traineeId && e.TraineeGroupId == traineeGroupId && e.IsActive)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync(ct);

        public async Task<PagedData<EnrollmentCardDto>> SearchAsync(string term, PageRequest page, CancellationToken ct = default)
        {
            var query = _context.Enrollments
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.AppUser)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Employee)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Sport)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Branch)
                .Include(e => e.SubscriptionDetails)
                    .ThenInclude(sd => sd.SportPrice)
                        .ThenInclude(sp => sp.SportSubscriptionType)
                            .ThenInclude(sst => sst.SubscriptionType)
                .Include(e => e.SubscriptionDetails)
                    .ThenInclude(sd => sd.Payment)
                .Include(e => e.Attendances)
                .Where(e => e.Trainee.FirstName.Contains(term)
                    || e.Trainee.LastName.Contains(term)
                    || (e.Trainee.FirstName + " " + e.Trainee.LastName).Contains(term)
                    || e.TraineeGroup.Coach.Employee.FirstName.Contains(term)
                    || e.TraineeGroup.Coach.Employee.LastName.Contains(term)
                    || (e.TraineeGroup.Coach.Employee.FirstName + " " + e.TraineeGroup.Coach.Employee.LastName).Contains(term)
                    || e.TraineeGroup.Branch.Name.Contains(term)
                    || e.TraineeGroup.Coach.Sport.Name.Contains(term))
                .AsNoTracking();

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(ct);

            var dtos = items.Select(e =>
            {
                var sportName = e.TraineeGroup.Coach.Sport.Name;
                var programName = e.SubscriptionDetails?.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString();
                var monthlyFee = e.SubscriptionDetails?.SportPrice?.Price ?? 0;
                var hasPayment = e.SubscriptionDetails?.Payment != null;
                var sessionsCompleted = e.Attendances.Count(a =>
                    a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Present ||
                    a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Late);

                string paymentStatus;
                if (e.ExpiryDate < DateTime.UtcNow) paymentStatus = "Overdue";
                else if (hasPayment) paymentStatus = "Paid";
                else paymentStatus = "Pending";

                string status;
                if (e.ExpiryDate < DateTime.UtcNow) status = "Expired";
                else if (!e.IsActive) status = "Suspended";
                else status = "Active";

                return new EnrollmentCardDto(
                    e.Id,
                    $"{e.Trainee.FirstName} {e.Trainee.LastName}",
                    e.Trainee.AppUser?.Email,
                    sportName,
                    programName,
                    e.TraineeGroup.Branch.Name,
                    $"{e.TraineeGroup.Coach.Employee.FirstName} {e.TraineeGroup.Coach.Employee.LastName}",
                    e.EnrollmentDate.ToString("yyyy-MM-dd"),
                    e.SubscriptionDetails?.StartDate.ToString("yyyy-MM-dd"),
                    e.SubscriptionDetails?.EndDate.ToString("yyyy-MM-dd"),
                    monthlyFee,
                    paymentStatus,
                    status,
                    sessionsCompleted,
                    e.SessionAllowed
                );
            }).ToList();

            return new PagedData<EnrollmentCardDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<int> CountAllAsync(CancellationToken ct = default)
            => await _context.Enrollments.CountAsync(ct);

        public async Task<int> CountActiveAsync(CancellationToken ct = default)
            => await _context.Enrollments.CountAsync(e => e.IsActive, ct);

        public async Task<int> CountPendingPaymentAsync(CancellationToken ct = default)
            => await _context.Enrollments
                .Include(e => e.SubscriptionDetails)
                    .ThenInclude(s => s.Payment)
                .Where(e => e.SubscriptionDetails.Payment == null)
                .CountAsync(ct);

        public async Task<PagedData<EnrollmentCardDto>> GetAllAsync(PageRequest page, CancellationToken ct = default)
        {
            var query = _context.Enrollments
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.AppUser)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Employee)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Sport)
                .Include(e => e.TraineeGroup)
                    .ThenInclude(g => g.Branch)
                .Include(e => e.SubscriptionDetails)
                    .ThenInclude(sd => sd.SportPrice)
                        .ThenInclude(sp => sp.SportSubscriptionType)
                            .ThenInclude(sst => sst.SubscriptionType)
                .Include(e => e.SubscriptionDetails)
                    .ThenInclude(sd => sd.Payment)
                .Include(e => e.Attendances)
                .AsNoTracking();

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(ct);

            var dtos = items.Select(e =>
            {
                var sportName = e.TraineeGroup.Coach.Sport.Name;
                var programName = e.SubscriptionDetails?.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString();
                var monthlyFee = e.SubscriptionDetails?.SportPrice?.Price ?? 0;
                var hasPayment = e.SubscriptionDetails?.Payment != null;
                var sessionsCompleted = e.Attendances.Count(a =>
                    a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Present ||
                    a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Late);

                string paymentStatus;
                if (e.ExpiryDate < DateTime.UtcNow) paymentStatus = "Overdue";
                else if (hasPayment) paymentStatus = "Paid";
                else paymentStatus = "Pending";

                string status;
                if (e.ExpiryDate < DateTime.UtcNow) status = "Expired";
                else if (!e.IsActive) status = "Suspended";
                else status = "Active";

                return new EnrollmentCardDto(
                    e.Id,
                    $"{e.Trainee.FirstName} {e.Trainee.LastName}",
                    e.Trainee.AppUser?.Email,
                    sportName,
                    programName,
                    e.TraineeGroup.Branch.Name,
                    $"{e.TraineeGroup.Coach.Employee.FirstName} {e.TraineeGroup.Coach.Employee.LastName}",
                    e.EnrollmentDate.ToString("yyyy-MM-dd"),
                    e.SubscriptionDetails?.StartDate.ToString("yyyy-MM-dd"),
                    e.SubscriptionDetails?.EndDate.ToString("yyyy-MM-dd"),
                    monthlyFee,
                    paymentStatus,
                    status,
                    sessionsCompleted,
                    e.SessionAllowed
                );
            }).ToList();

            return new PagedData<EnrollmentCardDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<EnrollmentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct = default)
        {
            var e = await _context.Enrollments
                .Include(en => en.Trainee)
                    .ThenInclude(t => t.AppUser)
                .Include(en => en.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Employee)
                .Include(en => en.TraineeGroup)
                    .ThenInclude(g => g.Coach)
                        .ThenInclude(c => c.Sport)
                .Include(en => en.TraineeGroup)
                    .ThenInclude(g => g.Branch)
                .Include(en => en.SubscriptionDetails)
                    .ThenInclude(sd => sd.SportPrice)
                        .ThenInclude(sp => sp.SportSubscriptionType)
                            .ThenInclude(sst => sst.SubscriptionType)
                .Include(en => en.SubscriptionDetails)
                    .ThenInclude(sd => sd.Payment)
                .Include(en => en.Attendances)
                .AsNoTracking()
                .FirstOrDefaultAsync(en => en.Id == id, ct);

            if (e == null) return null;

            var sessionsCompleted = e.Attendances.Count(a =>
                a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Present ||
                a.AttendanceStatus == SportAcademy.Domain.Enums.AttendanceStatus.Late);
            var hasPayment = e.SubscriptionDetails?.Payment != null;

            string paymentStatus;
            if (e.ExpiryDate < DateTime.UtcNow) paymentStatus = "Overdue";
            else if (hasPayment) paymentStatus = "Paid";
            else paymentStatus = "Pending";

            string status;
            if (e.ExpiryDate < DateTime.UtcNow) status = "Expired";
            else if (!e.IsActive) status = "Suspended";
            else status = "Active";

            return new EnrollmentDetailDto(
                e.Id,
                $"{e.Trainee.FirstName} {e.Trainee.LastName}",
                e.Trainee.AppUser?.Email,
                e.TraineeGroup.Coach.Sport.Name,
                e.SubscriptionDetails?.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString(),
                e.TraineeGroup.Branch.Name,
                $"{e.TraineeGroup.Coach.Employee.FirstName} {e.TraineeGroup.Coach.Employee.LastName}",
                e.EnrollmentDate.ToString("yyyy-MM-dd"),
                e.SubscriptionDetails?.StartDate.ToString("yyyy-MM-dd"),
                e.SubscriptionDetails?.EndDate.ToString("yyyy-MM-dd"),
                e.ExpiryDate.ToString("yyyy-MM-dd"),
                e.SubscriptionDetails?.SportPrice?.Price ?? 0,
                paymentStatus,
                status,
                sessionsCompleted,
                e.SessionAllowed - e.SessionRemaining,
                e.SessionAllowed,
                e.SubscriptionDetailsId
            );
        }
    }
}
