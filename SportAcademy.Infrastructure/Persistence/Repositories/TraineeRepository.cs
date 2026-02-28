using Azure;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeRepository : BaseRepository<Trainee, int>, ITraineeRepository
    {
        private readonly ApplicationDbContext _context;

        public TraineeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedData<TraineeOfSpecificDayDto>> GetAllTraineesOfSpecificDayAsync(
                DateTime date,
                PageRequest page,
                CancellationToken ct = default
            )
        {
            var rows = await (
                from t in _context.Trainees.AsNoTracking()
                where t.Enrollments.Any(e => e.TraineeGroup.GroupSchedules.Any(gs => gs.Day == date.DayOfWeek))

                from ts in t.Sports
                from sb in ts.Sport.Branches

                select new
                {
                    TraineeId = t.Id,
                    FullName = $"{t.FirstName} + {t.LastName}",
                    Age = DateTime.Now.Year - t.BirthDate.Year,
                    t.Email,
                    t.PhoneNumber,
                    t.JoinDate,
                    t.IsSubscribed,

                    SportName = ts.Sport.Name,
                    ts.SkillLevel,
                    BranchName = sb.Branch.Name
                }
            ).ToListAsync(ct);

            var result = rows
                .GroupBy(r => new
                {
                    r.TraineeId,
                    r.FullName,
                    r.Age,
                    r.Email,
                    r.PhoneNumber,
                    r.JoinDate,
                    r.IsSubscribed,
                })
                .Select(traineeGroup => new TraineeOfSpecificDayDto(
                    traineeGroup.Key.TraineeId,
                    traineeGroup.Key.FullName,
                    traineeGroup.Key.Age,
                    traineeGroup.Key.Email.ToString(),
                    traineeGroup.Key.PhoneNumber,
                    traineeGroup.Key.JoinDate,
                    traineeGroup.Key.IsSubscribed,
                    0,

                    traineeGroup
                        .GroupBy(x => new { x.SportName, x.SkillLevel })
                        .Select(sportGroup => new TraineeSportDto(
                            sportGroup.Key.SportName,
                            sportGroup.Key.SkillLevel,
                            sportGroup
                                .Select(x => x.BranchName)
                                .Distinct()
                                .ToList()
                        ))
                        .ToList()
                ))
                .ToPagedData(page);

            return result;
        }

        public async Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => t.Id == id)
                .Include(t => t.AppUser)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<int> GetTraineesCountOfSpecificDayAsync(DateTime date, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => t.Enrollments.Any(e => e.TraineeGroup.GroupSchedules.Any(gs => gs.Day == date.DayOfWeek)))
                .CountAsync(cancellationToken);

        public async Task<bool> IsPhoneNumberExistAsync(string phoneNumber, int excludeTraineeId = 0, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(e => e.Id != excludeTraineeId)
                .AnyAsync(e => e.PhoneNumber == phoneNumber, cancellationToken);

        public async Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default)
            => await _context.Trainees.AnyAsync(t => t.SSN == ssn, cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Trainees.CountAsync(cancellationToken);
        }
    }
}
