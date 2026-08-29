using SportAcademy.Domain.Exceptions.BaseExceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeRepository : BaseRepository<Trainee, int>, ITraineeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantIdProvider _tenantIdProvider;

        public TraineeRepository(ApplicationDbContext context, IMapper mapper, ITenantIdProvider tenantIdProvider)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _tenantIdProvider = tenantIdProvider;
        }

        public async Task<List<int>> GetIdsAsync(CancellationToken ct = default)
            => await _context.Trainees
                .Select(t => t.Id)
                .OrderDescending()
                .ToListAsync(cancellationToken: ct);

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
                    IsSubscribed = t.SubscriptionDetails.Any(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted),

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
            => await _context.SubscriptionDetails
                .Where(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted)
                .Select(sd => sd.TraineeId)
                .Distinct()
                .CountAsync(cancellationToken);

        private static async Task<bool> IsFtsAvailableAsync(IDbConnection connection, string tableName)
        {
            var result = await connection.QuerySingleAsync<int>(@"
                SELECT CASE
                    WHEN SERVERPROPERTY('IsFullTextInstalled') = 1
                        AND EXISTS (
                            SELECT 1 FROM sys.fulltext_indexes fi
                            JOIN sys.objects o ON fi.object_id = o.object_id
                            WHERE o.name = @tableName
                        )
                    THEN 1 ELSE 0
                END", new { tableName });
            return result == 1;
        }

        public async Task<PagedData<TraineeCardDto>> SearchAsync(
            string term,
            PageRequest page,
            int? sportId = null,
            string? status = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default)
        {
            var offset = (page.Page - 1) * page.PageSize;
            var fullTextTerm = BuildFullTextTerm(term);

            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(ct);

            var ftsAvailable = await IsFtsAvailableAsync(connection, "Trainees");

            var hasTerm = !string.IsNullOrWhiteSpace(term);
            var hasSport = sportId.HasValue;
            var hasStatus = !string.IsNullOrWhiteSpace(status);

            var filterConditions = new List<string>();
            var filterParams = new Dictionary<string, object>
            {
                ["offset"] = offset,
                ["pageSize"] = page.PageSize,
                ["tenantId"] = _tenantIdProvider.TenantId!
            };

            if (hasSport)
            {
                // Matched by id, not by name: a name comparison broke the moment a sport's
                // displayed name could differ from what is stored (Arabic translations) - the id
                // is the only value the UI and the row agree on. No outer join/alias is needed;
                // EXISTS also correctly matches "has this sport among possibly several".
                filterConditions.Add(@"EXISTS (
                    SELECT 1 FROM SportTrainees st_filter
                    WHERE st_filter.TraineeId = t.Id AND st_filter.SportId = @sportId)");
                filterParams["sportId"] = sportId!.Value;
            }

            if (hasStatus)
            {
                if (status!.Equals("active", StringComparison.OrdinalIgnoreCase))
                    filterConditions.Add("EXISTS (SELECT 1 FROM SubscriptionDetails sd WHERE sd.TraineeId = t.Id AND sd.Status = N'Active' AND sd.IsDeleted = 0)");
                else
                    filterConditions.Add("NOT EXISTS (SELECT 1 FROM SubscriptionDetails sd WHERE sd.TraineeId = t.Id AND sd.Status = N'Active' AND sd.IsDeleted = 0)");
            }

            string sql;
            string countSql;
            int totalCount;

            if (hasTerm && ftsAvailable)
            {
                filterParams["term"] = fullTextTerm;
                var filterClause = filterConditions.Count > 0 ? "AND " + string.Join(" AND ", filterConditions) : "";

                var fromJoinWhere = $@"
                    FROM Trainees t
                    INNER JOIN CONTAINSTABLE(
                        Trainees,
                        (FirstName, LastName, Email),
                        @term, LANGUAGE 1025
                    ) ft ON t.Id = ft.[KEY]
                    LEFT JOIN Branches b ON t.BranchId = b.Id
                    WHERE t.IsDeleted = 0 AND t.TenantId = @tenantId
                    {filterClause}";

                countSql = $"SELECT COUNT(DISTINCT t.Id) {fromJoinWhere}";
                sql = $@"
                    SELECT 
                        t.Id,
                        t.TraineeCode AS Code,
                        t.FirstName,
                        t.LastName,
                        DATEDIFF(YEAR, t.BirthDate, GETDATE()) AS Age,
                        t.Email,
                        t.PhoneNumber,
                        t.JoinDate,
                        CASE WHEN EXISTS (
                            SELECT 1 FROM SubscriptionDetails sd
                            WHERE sd.TraineeId = t.Id AND sd.Status = N'Active' AND sd.IsDeleted = 0
                        ) THEN 1 ELSE 0 END AS IsSubscribed,
                        (SELECT 
                                s.Name AS SportName,
                                st.SkillLevel AS SkillLevel
                            FROM SportTrainees st
                            INNER JOIN Sports s 
                                ON st.SportId = s.Id
                            WHERE st.TraineeId = t.Id
                            FOR JSON PATH
                        ) AS SportSkills,
                        (SELECT TOP 1 ce.FirstName + ' ' + ce.LastName
                            FROM Enrollments e
                            INNER JOIN TraineeGroups tg ON tg.Id = e.TraineeGroupId
                            INNER JOIN Coaches c ON tg.CoachId = c.EmployeeId
                            INNER JOIN Employees ce ON ce.Id = c.EmployeeId
                            WHERE e.TraineeId = t.Id
                        ) AS CoachName,
                        b.Name AS BranchName,
                        (SELECT 
                                mc.Condition
                            FROM TraineeMedicalConditions mc
                            WHERE mc.TraineeId = t.Id
                            FOR JSON PATH
                        ) AS MedicalConditions
                    {fromJoinWhere}
                    GROUP BY
                        t.Id, t.TraineeCode, t.FirstName, t.LastName, t.BirthDate, t.Email,
                        t.PhoneNumber, t.JoinDate,
                        b.Name, ft.RANK
                    {BuildSortClause(sortBy, sortDir, "ft.RANK DESC, t.Id ASC")}
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
                ";
            }
            else
            {
                if (hasTerm)
                {
                    // A single "%term%" against each column separately (the old behavior) only
                    // matched a search string that happened to sit entirely inside one column -
                    // typing "Bandar Man" against FirstName="Bandar"/LastName="Mansour" never
                    // matched anything, since neither column alone contains "Bandar Man". Split
                    // into whitespace tokens and require each token to appear somewhere (first
                    // name, last name, their concatenation, or email) - order- and
                    // column-independent, so "Bandar Man", "Man Bandar", or just "Man" all match.
                    // Both sides go through dbo.NormalizeArabicText so "أحمد"/"احمد" (a name
                    // spelled two common ways) match regardless of which one the trainee record
                    // or the search box holds - the SQL function is the single source of truth
                    // for the folding rules, so column and term can never drift out of sync.
                    var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var tokenConditions = new List<string>();
                    for (var i = 0; i < tokens.Length; i++)
                    {
                        var p = $"likeTerm{i}";
                        tokenConditions.Add(
                            $"(dbo.NormalizeArabicText(t.FirstName) LIKE dbo.NormalizeArabicText(@{p}) OR " +
                            $"dbo.NormalizeArabicText(t.LastName) LIKE dbo.NormalizeArabicText(@{p}) OR " +
                            $"dbo.NormalizeArabicText(t.FirstName + ' ' + t.LastName) LIKE dbo.NormalizeArabicText(@{p}) OR " +
                            $"t.Email LIKE @{p})");
                        filterParams[p] = $"%{tokens[i]}%";
                    }
                    if (tokenConditions.Count > 0)
                        filterConditions.Add(string.Join(" AND ", tokenConditions));
                }
                var filterClause = filterConditions.Count > 0 ? "AND " + string.Join(" AND ", filterConditions) : "";

                var fromJoinWhere = $@"
                    FROM Trainees t
                    LEFT JOIN Branches b ON t.BranchId = b.Id
                    WHERE t.IsDeleted = 0 AND t.TenantId = @tenantId
                    {filterClause}";

                countSql = $"SELECT COUNT(DISTINCT t.Id) {fromJoinWhere}";
                sql = $@"
                    SELECT 
                        t.Id,
                        t.TraineeCode AS Code,
                        t.FirstName,
                        t.LastName,
                        DATEDIFF(YEAR, t.BirthDate, GETDATE()) AS Age,
                        t.Email,
                        t.PhoneNumber,
                        t.JoinDate,
                        CASE WHEN EXISTS (
                            SELECT 1 FROM SubscriptionDetails sd
                            WHERE sd.TraineeId = t.Id AND sd.Status = N'Active' AND sd.IsDeleted = 0
                        ) THEN 1 ELSE 0 END AS IsSubscribed,
                        (SELECT 
                                s.Name AS SportName,
                                st.SkillLevel AS SkillLevel
                            FROM SportTrainees st
                            INNER JOIN Sports s 
                                ON st.SportId = s.Id
                            WHERE st.TraineeId = t.Id
                            FOR JSON PATH
                        ) AS SportSkills,
                        (SELECT TOP 1 ce.FirstName + ' ' + ce.LastName
                            FROM Enrollments e
                            INNER JOIN TraineeGroups tg ON tg.Id = e.TraineeGroupId
                            INNER JOIN Coaches c ON tg.CoachId = c.EmployeeId
                            INNER JOIN Employees ce ON ce.Id = c.EmployeeId
                            WHERE e.TraineeId = t.Id
                        ) AS CoachName,
                        b.Name AS BranchName,
                        (SELECT 
                                mc.Condition
                            FROM TraineeMedicalConditions mc
                            WHERE mc.TraineeId = t.Id
                            FOR JSON PATH
                        ) AS MedicalConditions
                    {fromJoinWhere}
                    GROUP BY
                        t.Id, t.TraineeCode, t.FirstName, t.LastName, t.BirthDate, t.Email,
                        t.PhoneNumber, t.JoinDate,
                        b.Name
                    {BuildSortClause(sortBy, sortDir, "t.Id ASC")}
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
                ";
            }

            var parameters = new DynamicParameters(filterParams);
            var multi = await connection.QueryMultipleAsync($"{countSql}; {sql}", parameters);
            totalCount = await multi.ReadSingleAsync<int>();
            var rows = (await multi.ReadAsync<TraineeCardRow>()).ToList();

            var trainees = rows.Select(r =>
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(null, true) }
                };

                var sports = string.IsNullOrWhiteSpace(r.SportSkills)
                    ? []
                    : JsonSerializer.Deserialize<List<TraineeSportSkillDto>>(
                        r.SportSkills,
                        options
                        );

                var medicalConditions = string.IsNullOrWhiteSpace(r.MedicalConditions)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<MedicalConditionRow>>(r.MedicalConditions)
                        ?.Select(x => x.Condition).ToList() ?? [];

                return new TraineeCardDto(
                    r.Id,
                    r.Code,
                    r.FirstName,
                    r.LastName,
                    r.Age,
                    r.Email,
                    r.PhoneNumber,
                    r.JoinDate,
                    r.IsSubscribed,
                    sports ?? [],
                    r.CoachName,
                    r.BranchName
                )
                {
                    MedicalConditions = medicalConditions
                };
            }).ToList();

            return new PagedData<TraineeCardDto>
            {
                Items = trainees,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
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
                .Include(t => t.MedicalConditions)
                .AsNoTracking()
                .ProjectTo<TraineeDetailsDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken)
                // IdNotFoundException, not KeyNotFoundException: the pipeline maps the former to a
                // localized 404, while the latter fell through to the generic 500 handler.
                ?? throw new IdNotFoundException(nameof(Trainee), id);

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

        public async Task<List<TraineeDropdownDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => !t.IsDeleted)
                .AsNoTracking()
                .ProjectTo<TraineeDropdownDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
            => await _context.Trainees
                .Where(t => t.Id != 0)
                .AnyAsync(t => t.Email.Value == email.ToLowerInvariant(), cancellationToken);

        public async Task<List<TraineeExportDto>> GetExportDataByIdsAsync(List<int> ids, CancellationToken ct = default)
        {
            var trainees = await _context.Trainees
                .AsNoTracking()
                .Where(t => ids.Contains(t.Id))
                .Include(t => t.Branch)
                .Include(t => t.Family)
                .Include(t => t.NationalityCategory)
                .Include(t => t.Sports).ThenInclude(st => st.Sport)
                .Include(t => t.Enrollments).ThenInclude(e => e.TraineeGroup)
                .Include(t => t.SubscriptionDetails)
                    .ThenInclude(sd => sd.SportPrice)
                    .ThenInclude(sp => sp.SportSubscriptionType)
                    .ThenInclude(sst => sst.SubscriptionType)
                .Include(t => t.MedicalConditions)
                .ToListAsync(ct);

            return trainees.Select(t => new TraineeExportDto
            {
                Id = t.Id,
                TraineeCode = t.TraineeCode.Value,
                FirstName = t.FirstName,
                LastName = t.LastName,
                SSN = t.SSN,
                Email = t.Email.Value,
                PhoneNumber = t.PhoneNumber,
                SecondPhoneNumber = t.SecondPhoneNumber,
                BirthDate = t.BirthDate,
                Age = t.GetAge(),
                Gender = t.Gender.ToString(),
                Nationality = t.Nationality.ToString(),
                GuardianName = t.GuardianName,
                ParentNumber = t.ParentNumber,
                Street = t.Address.Street,
                City = t.Address.City,
                JoinDate = t.JoinDate.ToDateTime(TimeOnly.MinValue),
                BranchName = t.Branch.Name,
                FamilyId = t.FamilyId,
                FamilyCode = t.Family.FamilyCode,
                NationalityCategoryName = t.NationalityCategory.Name,
                IsSubscribed = t.SubscriptionDetails.Any(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted),
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                SportNames = string.Join("|", t.Sports.Select(s => s.Sport.Name)),
                SkillLevels = string.Join("|", t.Sports.Select(s => s.SkillLevel.ToString())),
                LatestEnrollmentDate = t.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Select(e => DateOnly.FromDateTime(e.EnrollmentDate))
                    .Cast<DateOnly?>()
                    .FirstOrDefault(),
                LatestExpiryDate = t.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Select(e => DateOnly.FromDateTime(e.ExpiryDate))
                    .Cast<DateOnly?>()
                    .FirstOrDefault(),
                LatestSessionAllowed = t.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Select(e => (int?)e.SessionAllowed)
                    .FirstOrDefault(),
                LatestSessionRemaining = t.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Select(e => (int?)e.SessionRemaining)
                    .FirstOrDefault(),
                LatestGroupName = t.Enrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Select(e => e.TraineeGroup.Name)
                    .FirstOrDefault(),
                LatestSubscriptionStartDate = t.SubscriptionDetails
                    .OrderByDescending(sd => sd.StartDate)
                    .Select(sd => (DateOnly?)sd.StartDate)
                    .FirstOrDefault(),
                LatestSubscriptionEndDate = t.SubscriptionDetails
                    .OrderByDescending(sd => sd.StartDate)
                    .Select(sd => (DateOnly?)sd.EndDate)
                    .FirstOrDefault(),
                LatestSubscriptionType = t.SubscriptionDetails
                    .OrderByDescending(sd => sd.StartDate)
                    .Select(sd => sd.SportPrice.SportSubscriptionType.SubscriptionType.Name.ToString())
                    .FirstOrDefault(),
                LatestSubscriptionStatus = t.SubscriptionDetails
                    .OrderByDescending(sd => sd.StartDate)
                    .Select(sd => sd.Status.ToString())
                    .FirstOrDefault(),
                MedicalConditions = t.MedicalConditions.Any()
                    ? string.Join("|", t.MedicalConditions.Select(mc => mc.Condition))
                    : null,
            }).ToList();
        }

        private static string BuildSortClause(string? sortBy, string? sortDir, string defaultSort)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return $"ORDER BY {defaultSort}";

            var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            var sortColumn = sortBy.ToLowerInvariant() switch
            {
                "name" => "t.FirstName, t.LastName",
                "branch" => "b.Name",
                "joined" => "t.JoinDate",
                _ => null
            };

            return sortColumn == null
                ? $"ORDER BY {defaultSort}"
                : $"ORDER BY {string.Join(", ", sortColumn.Split(',', StringSplitOptions.TrimEntries).Select(c => $"{c} {dir}"))}";
        }

        private class MedicalConditionRow
        {
            public string Condition { get; set; } = null!;
        }
    }
}
