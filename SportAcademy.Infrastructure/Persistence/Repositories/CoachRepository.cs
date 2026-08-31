using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;
using System.Data;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class CoachRepository : BaseRepository<Coach, int>, ICoachRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly ICurrentLanguageProvider _languageProvider;

        public CoachRepository(ApplicationDbContext context, IMapper mapper, ITenantIdProvider tenantIdProvider, ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _tenantIdProvider = tenantIdProvider;
            _languageProvider = languageProvider;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Coachs.CountAsync(cancellationToken);
        }
        public async Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Coachs
                .Select(x => (double?)x.Rate)
                .AverageAsync(cancellationToken);
        }

        public async Task<PagedData<CoachCardDto>> SearchAsync(
            string term,
            PageRequest pageReq,
            CancellationToken cancellationToken)
        {
            var offset = (pageReq.Page - 1) * pageReq.PageSize;
            var fullTextTerm = BuildFullTextTerm(term);
            var likeTerm = $"%{term}%";

            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var ftsAvailable = await connection.QuerySingleAsync<int>(@"
                SELECT CASE
                    WHEN SERVERPROPERTY('IsFullTextInstalled') = 1
                        AND EXISTS (
                            SELECT 1 FROM sys.fulltext_indexes fi
                            JOIN sys.objects o ON fi.object_id = o.object_id
                            WHERE o.name = 'Employees'
                        )
                    THEN 1 ELSE 0
                END") == 1;

            string sql;
            object parameters;

            if (ftsAvailable)
            {
                sql = @"
                    SELECT
                        c.EmployeeId AS Id,
                        e.FirstName,
                        e.LastName,
                        e.Position,
                        ISNULL(bt.Name, b.Name) AS BranchName,
                        e.Email,
                        e.IsWork,
                        e.PhoneNumber,
                        (e.City + ', ' + e.Street) AS Address,
                        e.HireDate,
                        ISNULL(trainee_count.TotalTrainees, 0) AS TotalTrainees,
                        c.SkillLevel,
                        ISNULL(st.Name, s.Name) AS SportName
                    FROM Coaches c
                    INNER JOIN Employees e ON c.EmployeeId = e.Id
                    INNER JOIN CONTAINSTABLE(
                        Employees,
                        (FirstName, LastName),
                        @term, LANGUAGE 1025
                    ) ft ON e.Id = ft.[KEY]
                    INNER JOIN Branches b ON e.BranchId = b.Id
                    LEFT JOIN BranchTranslations bt ON bt.BranchId = b.Id AND bt.LangCode = @lang
                    INNER JOIN Sports s ON c.SportId = s.Id
                    LEFT JOIN SportTranslations st ON st.SportId = s.Id AND st.LangCode = @lang
                    LEFT JOIN (
                        SELECT
                            tg.CoachId,
                            COUNT(enr.Id) AS TotalTrainees
                        FROM TraineeGroups tg
                        LEFT JOIN Enrollments enr ON tg.Id = enr.TraineeGroupId
                            AND enr.IsActive = 1
                            AND enr.IsDeleted = 0
                        GROUP BY tg.CoachId
                    ) trainee_count ON c.EmployeeId = trainee_count.CoachId
                    WHERE e.TenantId = @tenantId
                    ORDER BY ft.RANK DESC, c.EmployeeId ASC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM Coaches c
                    INNER JOIN Employees e ON c.EmployeeId = e.Id
                    INNER JOIN CONTAINSTABLE(
                        Employees,
                        (FirstName, LastName),
                        @term, LANGUAGE 1025
                    ) ft ON e.Id = ft.[KEY]
                    WHERE e.TenantId = @tenantId;
                ";
                parameters = new { term = fullTextTerm, offset, pageReq.PageSize, tenantId = _tenantIdProvider.TenantId, lang = _languageProvider.Language };
            }
            else
            {
                sql = @"
                    SELECT
                        c.EmployeeId AS Id,
                        e.FirstName,
                        e.LastName,
                        e.Position,
                        ISNULL(bt.Name, b.Name) AS BranchName,
                        e.Email,
                        e.IsWork,
                        e.PhoneNumber,
                        (e.City + ', ' + e.Street) AS Address,
                        e.HireDate,
                        ISNULL(trainee_count.TotalTrainees, 0) AS TotalTrainees,
                        c.SkillLevel,
                        ISNULL(st.Name, s.Name) AS SportName
                    FROM Coaches c
                    INNER JOIN Employees e ON c.EmployeeId = e.Id
                    INNER JOIN Branches b ON e.BranchId = b.Id
                    LEFT JOIN BranchTranslations bt ON bt.BranchId = b.Id AND bt.LangCode = @lang
                    INNER JOIN Sports s ON c.SportId = s.Id
                    LEFT JOIN SportTranslations st ON st.SportId = s.Id AND st.LangCode = @lang
                    LEFT JOIN (
                        SELECT
                            tg.CoachId,
                            COUNT(enr.Id) AS TotalTrainees
                        FROM TraineeGroups tg
                        LEFT JOIN Enrollments enr ON tg.Id = enr.TraineeGroupId
                            AND enr.IsActive = 1
                            AND enr.IsDeleted = 0
                        GROUP BY tg.CoachId
                    ) trainee_count ON c.EmployeeId = trainee_count.CoachId
                    WHERE e.TenantId = @tenantId AND (e.FirstName LIKE @likeTerm OR e.LastName LIKE @likeTerm)
                    ORDER BY c.EmployeeId ASC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM Coaches c
                    INNER JOIN Employees e ON c.EmployeeId = e.Id
                    WHERE e.TenantId = @tenantId AND (e.FirstName LIKE @likeTerm OR e.LastName LIKE @likeTerm);
                ";
                parameters = new { likeTerm, offset, pageReq.PageSize, tenantId = _tenantIdProvider.TenantId, lang = _languageProvider.Language };
            }

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var coaches = (await multi.ReadAsync<CoachCardDto>()).ToList();

            return coaches.ToPagedData(pageReq);
        }

        private static string BuildFullTextTerm(string term)
        {
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" AND ",
                tokens.Select(t => $"\"{t}*\""));
        }

        public async Task<Coach?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Coachs
                .Where(c => c.EmployeeId == id && !c.IsDeleted)
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Branch)
                        .ThenInclude(b => b.Translations)
                .Include(c => c.Sport)
                    .ThenInclude(s => s.Translations)
                .Include(c => c.TraineeGroups)
                    .ThenInclude(tg => tg.Enrollments.Where(e => e.IsActive && !e.IsDeleted))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CoachDropdownItemDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default)
            => await _context.Coachs
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .Select(CoachProjections.ToDropdownDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);
    }
}
