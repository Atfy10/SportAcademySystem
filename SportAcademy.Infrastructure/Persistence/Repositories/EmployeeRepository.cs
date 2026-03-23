using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using System.Data;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee, int>, IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeRepository(ApplicationDbContext context, IMapper mapper) 
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedData<CoachCardDto>> GetAllCoaches(PageRequest page, CancellationToken ct = default)
            => await _context.Coachs
                .AsNoTracking()
                .ProjectTo<CoachCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, ct);

        public async Task<int> GetActiveEmployeesCountAsync(CancellationToken ct = default)
            => await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsWork)
                .CountAsync(ct);

        public async Task<int> GetActiveCoachesCountAsync(CancellationToken ct = default)
            => await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsWork && e.Coach != null)
                .CountAsync(ct);

        public Task<PagedData<EmployeeCardDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.Employees
                .AsNoTracking()
                .ProjectTo<EmployeeCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<PagedData<EmployeeDto>> GetActiveAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.IsWork)
                .AsNoTracking()
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
            => await _context.Employees
                .Where(e => e.IsWork && e.Coach != null)
                .AsNoTracking()
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<PagedData<EmployeeDto>> GetCoachEmployeesWithoutCoachRecordAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.IsWork && e.Position == Domain.Enums.Position.Coach)
                .Where(e => !_context.Coachs.Any(c => c.EmployeeId == e.Id))
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<int> GetEmployeesCountAsync(CancellationToken ct = default)
            => await _context.Employees.CountAsync(ct);

        public async Task<PagedData<EmployeeCardDto>> SearchAsync(
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
                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.Position,
                        b.Name AS BranchName,
                        e.Email,
                        e.IsWork,
                        e.PhoneNumber,
                        (e.City + ', ' + e.Street) AS Address,
                        e.HireDate
                    FROM Employees e
                    INNER JOIN CONTAINSTABLE(
                        Employees,
                        (FirstName, LastName),
                        @term
                    ) ft ON e.Id = ft.[KEY]
                    INNER JOIN Branches b ON e.BranchId = b.Id
                    ORDER BY ft.RANK DESC, e.Id ASC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM CONTAINSTABLE(
                        Employees,
                        (FirstName, LastName),
                        @term
                    );
                ";
                parameters = new { term = fullTextTerm, offset, pageReq.PageSize };
            }
            else
            {
                sql = @"
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
                        e.HireDate
                    FROM Employees e
                    INNER JOIN Branches b ON e.BranchId = b.Id
                    WHERE (e.FirstName LIKE @likeTerm OR e.LastName LIKE @likeTerm)
                    ORDER BY e.Id ASC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                    SELECT COUNT(*)
                    FROM Employees e
                    WHERE (e.FirstName LIKE @likeTerm OR e.LastName LIKE @likeTerm);
                ";
                parameters = new { likeTerm, offset, pageReq.PageSize };
            }

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var employees = (await multi.ReadAsync<EmployeeCardDto>()).ToList();

            return employees.ToPagedData(pageReq);
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
