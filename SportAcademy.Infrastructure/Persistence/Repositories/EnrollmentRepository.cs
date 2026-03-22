using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public EnrollmentRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EnrollmentsSportDto> GetAllEnrollmentsForSport(
            PageRequest page,
            DateTime? from,
            DateTime? to,
            int sportId,
            CancellationToken ct = default)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var sportName = await _context.Sports
                .Where(s => s.Id == sportId)
                .Select(s => s.Name)
                .FirstAsync(ct);

            var query = _context.Enrollments
                .Where(e => e.TraineeGroup!.Coach!.SportId == sportId)
                .Where(e => e.EnrollmentDate >= fromDate && e.EnrollmentDate < toDate);

            var pagedData = await query
                .AsNoTracking()
                .ProjectTo<EnrollmentDataDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, ct);

            return new EnrollmentsSportDto(pagedData, sportName);
        }

        public async Task<int> GetEnrollmentsCountForSport(
            int sportId,
            DateTime? from,
            DateTime? to,
            CancellationToken ct = default)
            => await _context.Enrollments
                .Where(e => e.TraineeGroup!.Coach!.SportId == sportId)
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
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var sportsPage = await _context.Enrollments
                .Where(e => e.EnrollmentDate >= fromDate && e.EnrollmentDate < toDate)
                .Select(e => e.TraineeGroup!.Coach!.Sport!.Name)
                .Distinct()
                .OrderBy(s => s)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(ct);

            var enrollments = await _context.Enrollments
                .Where(e => sportsPage.Contains(e.TraineeGroup!.Coach!.Sport!.Name))
                .Where(e => e.EnrollmentDate >= fromDate && e.EnrollmentDate < toDate)
                .AsNoTracking()
                .ProjectTo<EnrollmentDataDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            var grouped = enrollments
                .GroupBy(e => _context.Enrollments
                    .Where(en => en.Id == e.Id)
                    .Select(en => en.TraineeGroup!.Coach!.Sport!.Name)
                    .First())
                .Select(g => new EnrollmentsSportsDto(g.ToList(), g.Key))
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
                .Where(e => e.Trainee.FirstName.Contains(term)
                    || e.Trainee.LastName.Contains(term)
                    || (e.Trainee.FirstName + " " + e.Trainee.LastName).Contains(term)
                    || e.TraineeGroup!.Coach!.Employee!.FirstName.Contains(term)
                    || e.TraineeGroup.Coach.Employee.LastName.Contains(term)
                    || (e.TraineeGroup.Coach.Employee.FirstName + " " + e.TraineeGroup.Coach.Employee.LastName).Contains(term)
                    || e.TraineeGroup.Branch!.Name.Contains(term)
                    || e.TraineeGroup.Coach.Sport!.Name.Contains(term))
                .AsNoTracking()
                .ProjectTo<EnrollmentCardDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, ct);
        }

        public async Task<int> CountAllAsync(CancellationToken ct = default)
            => await _context.Enrollments.CountAsync(ct);

        public async Task<int> CountActiveAsync(CancellationToken ct = default)
            => await _context.Enrollments.CountAsync(e => e.IsActive, ct);

        public async Task<int> CountPendingPaymentAsync(CancellationToken ct = default)
            => await _context.Enrollments
                .Where(e => e.SubscriptionDetails != null && e.SubscriptionDetails.Payment == null)
                .CountAsync(ct);

        public async Task<PagedData<EnrollmentCardDto>> GetAllAsync(PageRequest page, CancellationToken ct = default)
        {
            var query = _context.Enrollments
                .AsNoTracking()
                .ProjectTo<EnrollmentCardDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, ct);
        }

        public async Task<EnrollmentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct = default)
            => await _context.Enrollments
                .Where(en => en.Id == id)
                .AsNoTracking()
                .ProjectTo<EnrollmentDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
    }
}
