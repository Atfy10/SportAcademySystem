using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using System.Data;
using System.Text.Json;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeRepository : BaseRepository<Trainee, int>, ITraineeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TraineeRepository(ApplicationDbContext context, IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
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
                .Include(t => t.Sports)
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

        public async Task<int> GetActiveTraineesCount(CancellationToken cancellationToken = default)
            => await _context.Trainees.CountAsync(t => t.IsSubscribed, cancellationToken);

        public async Task<PagedData<TraineeCardDto>> SearchAsync(
            string term,
            PageRequest pageReq,
            CancellationToken cancellationToken)
        {
            var offset = (pageReq.Page - 1) * pageReq.PageSize;
            var fullTextTerm = BuildFullTextTerm(term);

            var sql = @"
                SELECT 
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    DATEDIFF(YEAR, t.BirthDate, GETDATE()) AS Age,
                    t.Email,
                    t.PhoneNumber,
                    t.JoinDate,
                    t.IsSubscribed,

                    (SELECT 
                            s.Name AS SportName,
                            st.SkillLevel AS SkillLevel
                        FROM SportTrainees st
                        INNER JOIN Sports s 
                            ON st.SportId = s.Id
                        WHERE st.TraineeId = t.Id
                        FOR JSON PATH
                    ) AS SportSkills,

                    (ce.FirstName + ' ' + ce.LastName) AS CoachName,

                    b.Name AS BranchName

                FROM Trainees t
                INNER JOIN CONTAINSTABLE(
                    Trainees,
                    (FirstName, LastName, Email),
                    @term
                ) ft ON t.Id = ft.[KEY]

                LEFT JOIN SportTrainees st ON t.Id = st.TraineeId
                LEFT JOIN Sports s ON st.SportId = s.Id

                LEFT JOIN Enrollments e ON e.TraineeId = t.Id
                LEFT JOIN TraineeGroups tg ON tg.Id = e.TraineeGroupId

                LEFT JOIN Coaches c ON tg.CoachId = c.EmployeeId
                LEFT JOIN Employees ce ON ce.Id = c.EmployeeId

                LEFT JOIN Branches b ON tg.BranchId = b.Id
                   
                WHERE t.IsDeleted = 0
                GROUP BY
                t.Id,
                t.FirstName,
                t.LastName,
                t.BirthDate,
                t.Email,
                t.PhoneNumber,
                t.JoinDate,
                t.IsSubscribed,
                ce.FirstName,
                ce.LastName,
                b.Name,
                ft.RANK

                ORDER BY ft.RANK DESC, t.Id ASC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
            ";


            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<TraineeCardRow>(sql, new
            {
                term = fullTextTerm,
                offset,
                pageReq.PageSize
            });

            var trainees = rows.Select(r =>
            {
                var sports = string.IsNullOrWhiteSpace(r.SportSkills)
                    ? []
                    : JsonSerializer.Deserialize<List<TraineeSportSkillDto>>(r.SportSkills)!;

                return new TraineeCardDto(
                    r.Id,
                    r.FirstName,
                    r.LastName,
                    r.Age,
                    r.Email,
                    r.PhoneNumber,
                    r.JoinDate,
                    r.IsSubscribed,
                    sports,
                    r.CoachName,
                    r.BranchName
                );
            }).ToList();


            //using var multi = await connection.QueryMultipleAsync(
            //    sql,
            //    new
            //    {
            //        term = fullTextTerm,
            //        offset,
            //        pageReq.PageSize
            //    });

            //var trainees = (await multi.ReadAsync<TraineeCardDto>()).ToList();

            return trainees.ToPagedData(pageReq);
        }

        private static string BuildFullTextTerm(string term)
        {
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" AND ",
                tokens.Select(t => $"\"{t}*\""));
        }

        public async Task<PagedData<TraineeCardDto>> SearchByIdAsync(int id, PageRequest page, CancellationToken ct = default)
            => await _context.Trainees
                .Where(t => t.Id == id)
                .AsNoTracking()
                .ProjectTo<TraineeCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, ct);

        public async Task<TraineeDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken)
            => await _context.Trainees
                .Where(t => t.Id == id)
                .AsNoTracking()
                .ProjectTo<TraineeDetailsDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Trainee with Id {id} not found.");

        public async Task<bool> IsLinkedToSport(int sportId, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .AnyAsync(t => t.Sports.Any(ts => ts.SportId == sportId), cancellationToken);

        public async Task<List<int>> GetSportIdsByTraineeId(int id, CancellationToken cancellationToken = default)
            => await _context.SportTrainees
                .Where(st => st.TraineeId == id)
                .Select(st => st.SportId)
                .ToListAsync(cancellationToken);

        public Task UpdateSports(Trainee trainee, IEnumerable<int> sportIds)
        {
            var current = trainee.Sports
                .Select(x => x.SportId)
                .ToHashSet();

            var requested = sportIds
                .Distinct()
                .ToHashSet();

            var toAdd = requested.Except(current).ToList();
            var toRemove = current.Except(requested).ToList();

            foreach (var entity in trainee.Sports
                .Where(x => toRemove.Contains(x.SportId))
                .ToList())
            {
                trainee.Sports.Remove(entity);
            }

            foreach (var sportId in toAdd)
            {
                trainee.Sports.Add(new SportTrainee
                {
                    SportId = sportId,
                    TraineeId = trainee.Id
                });
            }

            return Task.CompletedTask;
        }
    }
}
