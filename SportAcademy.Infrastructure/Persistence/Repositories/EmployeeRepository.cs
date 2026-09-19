using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;
using System.Data;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee, int>, IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly ICurrentLanguageProvider _languageProvider;

        public EmployeeRepository(ApplicationDbContext context, IMapper mapper, ITenantIdProvider tenantIdProvider, ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _tenantIdProvider = tenantIdProvider;
            _languageProvider = languageProvider;
        }

        public async Task<PagedData<CoachCardDto>> GetAllCoaches(PageRequest page, int? sportId = null, int? branchId = null, CancellationToken ct = default)
        {
            var query = _context.Coachs.AsNoTracking().AsQueryable();

            if (sportId.HasValue)
                query = query.Where(c => c.SportId == sportId.Value);
            if (branchId.HasValue)
                query = query.Where(c => c.Employee.BranchId == branchId.Value);

            return await query
                .OrderBy(c => c.EmployeeId)
                .Select(CoachProjections.ToCardDto(_languageProvider.Language))
                .ToPagedDataAsync(page, ct);
        }

        public async Task<int> GetActiveEmployeesCountAsync(CancellationToken ct = default)
            => await ApplyBranchFilter(_context.Employees)
                .AsNoTracking()
                .Where(e => e.IsWork)
                .CountAsync(ct);

        public async Task<int> GetActiveCoachesCountAsync(CancellationToken ct = default)
            => await ApplyBranchFilter(_context.Employees)
                .AsNoTracking()
                .Where(e => e.IsWork && e.Coach != null)
                .CountAsync(ct);

        public async Task<PagedData<EmployeeCardDto>> GetAllAsync(PageRequest page, EmployeeFilterOptions filters, CancellationToken cancellationToken = default)
        {
            var query = ApplyBranchFilter(_context.Employees)
                .Include(e => e.Branch)
                .AsNoTracking();

            if (filters.Status == "active")
                query = query.Where(e => e.IsWork);
            else if (filters.Status == "inactive")
                query = query.Where(e => !e.IsWork);

            if (filters.BranchId.HasValue)
                query = query.Where(e => e.BranchId == filters.BranchId);

            if (!string.IsNullOrEmpty(filters.Position) && Enum.TryParse<Domain.Enums.Position>(filters.Position, out var position))
                query = query.Where(e => e.Position == position);

            // Every branch needs an Id tiebreaker - none of these sort keys are unique
            // (multiple employees can share a name, position, branch, status, or hire date),
            // so without one, Skip/Take pagination can return rows inconsistently across pages.
            query = filters.SortBy?.ToLower() switch
            {
                "name" => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.FirstName + " " + e.LastName).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.FirstName + " " + e.LastName).ThenBy(e => e.Id),
                "position" => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Position).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Position).ThenBy(e => e.Id),
                "branch" => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Branch!.Name).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Branch!.Name).ThenBy(e => e.Id),
                "status" => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.IsWork).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.IsWork).ThenBy(e => e.Id),
                "hired" => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.HireDate).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.HireDate).ThenBy(e => e.Id),
                _ => filters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.Id)
            };

            return await query
                .ProjectTo<EmployeeCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<PagedData<EmployeeDto>> GetActiveAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await ApplyBranchFilter(_context.Employees)
                .Where(e => e.IsWork)
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<Employee?> GetFullEmployee(int id, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.Id == id)
                .Include(e => e.Branch)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<bool> IsPhoneNumberExistAsync(string phoneNumber, int excludeEmployeeId = 0, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.Id != excludeEmployeeId)
                .AnyAsync(e => e.PhoneNumber == phoneNumber, cancellationToken);

        public async Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default)
            => await _context.Employees.AnyAsync(e => e.SSN == ssn, cancellationToken);

        public async Task<PagedData<EmployeeDto>> GetActiveCoachesAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await ApplyBranchFilter(_context.Employees)
                .Where(e => e.IsWork && e.Coach != null)
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<PagedData<EmployeeDto>> GetCoachEmployeesWithoutCoachRecordAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await ApplyBranchFilter(_context.Employees)
                .Where(e => e.IsWork && e.Position == Domain.Enums.Position.Coach)
                .Where(e => !_context.Coachs.Any(c => c.EmployeeId == e.Id))
                .OrderBy(e => e.Id)
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<int> GetEmployeesCountAsync(CancellationToken ct = default)
            => await ApplyBranchFilter(_context.Employees).CountAsync(ct);

        public async Task<PagedData<EmployeeCardDto>> SearchAsync(
            string term,
            PageRequest pageReq,
            CancellationToken cancellationToken)
        {
            var offset = (pageReq.Page - 1) * pageReq.PageSize;
            var fullTextTerm = BuildFullTextTerm(term);

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

            var filterConditions = new List<string>();
            var filterParams = new Dictionary<string, object>
            {
                ["offset"] = offset,
                ["pageSize"] = pageReq.PageSize,
                ["tenantId"] = _tenantIdProvider.TenantId!
            };

            // baseJoin: what both the COUNT and the SELECT filter against. Branches is only
            // needed for the SELECT's display column, so it's added separately below.
            string baseJoin;
            string orderBy;

            if (ftsAvailable)
            {
                filterParams["term"] = fullTextTerm;
                baseJoin = @"
                    FROM Employees e
                    INNER JOIN CONTAINSTABLE(
                        Employees,
                        (FirstName, LastName),
                        @term, LANGUAGE 1025
                    ) ft ON e.Id = ft.[KEY]";
                orderBy = "ORDER BY ft.RANK DESC, e.Id ASC";
            }
            else
            {
                // Same fix as CoachRepository/TraineeRepository's fallback: tokenize instead of
                // matching the whole term as one substring against a single column, so a full
                // name ("John Smith") matches even though it's split across FirstName/LastName.
                var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var tokenConditions = new List<string>();
                for (var i = 0; i < tokens.Length; i++)
                {
                    var p = $"likeTerm{i}";
                    tokenConditions.Add(
                        $"(dbo.NormalizeArabicText(e.FirstName) LIKE dbo.NormalizeArabicText(@{p}) OR " +
                        $"dbo.NormalizeArabicText(e.LastName) LIKE dbo.NormalizeArabicText(@{p}) OR " +
                        $"dbo.NormalizeArabicText(e.FirstName + ' ' + e.LastName) LIKE dbo.NormalizeArabicText(@{p}))");
                    filterParams[p] = $"%{tokens[i]}%";
                }
                if (tokenConditions.Count > 0)
                    filterConditions.Add(string.Join(" AND ", tokenConditions));

                baseJoin = "FROM Employees e";
                orderBy = "ORDER BY e.Id ASC";
            }

            // e.IsDeleted = 0 was missing entirely before this fix (both branches only checked
            // TenantId), so a deleted employee - including one who was also a coach - still came
            // back in search.
            var whereClause = $@"
                WHERE e.TenantId = @tenantId AND e.IsDeleted = 0
                {(filterConditions.Count > 0 ? "AND " + string.Join(" AND ", filterConditions) : "")}";

            var countSql = $"SELECT COUNT(*) {baseJoin} {whereClause}";
            var sql = $@"
                SELECT
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.Position,
                    b.Name AS BranchName,
                    e.Email,
                    e.IsWork,
                    e.PhoneNumber,
                    (e.City + ', ' + e.Street) AS Address,
                    e.HireDate,
                    e.ImageUrl
                {baseJoin}
                INNER JOIN Branches b ON e.BranchId = b.Id
                {whereClause}
                {orderBy}
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

            var parameters = new DynamicParameters(filterParams);
            using var multi = await connection.QueryMultipleAsync($"{countSql}; {sql}", parameters);

            var totalCount = await multi.ReadSingleAsync<int>();
            var employees = (await multi.ReadAsync<EmployeeCardDto>()).ToList();

            return new PagedData<EmployeeCardDto>
            {
                Items = employees,
                TotalCount = totalCount,
                Page = pageReq.Page,
                PageSize = pageReq.PageSize
            };
        }

        private static string BuildFullTextTerm(string term)
        {
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" AND ",
                tokens.Select(t => $"\"{t}*\""));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The new state of employee</returns>
        /// <exception cref="EmployeeNotFoundException"></exception>
        public async Task<bool> ToggleIsWorkAsync(int id, CancellationToken cancellationToken = default)
        {
            var employee = await _context.Employees.FindAsync(new object[] { id }, cancellationToken)
                    ?? throw new EmployeeNotFoundException(id.ToString());

            employee.IsWork = !employee.IsWork;
            await _context.SaveChangesAsync(cancellationToken);
            return employee.IsWork;
        }

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.Id != 0)
                .AnyAsync(e => e.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }
}
