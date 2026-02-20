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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                PageRequest page,
                DateTime date,
                CancellationToken ct = default
            )
            => await _context.Trainees
                .Where(t => t.Enrollments.Any(e => e.TraineeGroup.GroupSchedules.Any(gs => gs.Day == date.DayOfWeek)))
                .Select(t => new TraineeOfSpecificDayDto(
                    t.Id,
                    $"{t.FirstName} {t.LastName}",
                    (DateTime.Now.Year - t.BirthDate.Year),
                    t.Email.ToString(),
                    t.PhoneNumber,
                    t.JoinDate,
                    t.IsSubscribed,
                    0,
                    new List<TraineeSportDto>{
                        new TraineeSportDto
                        {
                            SportName = t.Sports.Select(st=>st.Sport.Name),
                            SkillLevel = t.Enrollments.FirstOrDefault(e => e.TraineeGroup.GroupSchedules.Any(gs => gs.Day == date.DayOfWeek)).TraineeGroup.SkillLevel,
                            BranchName = t.Enrollments.FirstOrDefault(e => e.TraineeGroup.GroupSchedules.Any(gs => gs.Day == date.DayOfWeek)).TraineeGroup.Branch.Name
                        }
                    }
                ))
                .ToPagedDataAsync(page, ct);

        public async Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => t.Id == id)
                .Include(t => t.AppUser)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<bool> IsPhoneNumberExistAsync(string phoneNumber, int excludeTraineeId = 0, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(e => e.Id != excludeTraineeId)
                .AnyAsync(e => e.PhoneNumber == phoneNumber, cancellationToken);

        public async Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default)
            => await _context.Trainees.AnyAsync(t => t.SSN == ssn, cancellationToken);
    }
}
