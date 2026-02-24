using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
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

        public async Task<int> GetActiveEmployeesCountAsync(CancellationToken ct = default)
            => await _context.Employees
                .Where(e => e.IsWork)
                .CountAsync(ct);

        public async Task<int> GetActiveCoachesCountAsync(CancellationToken ct = default)
            => await _context.Employees
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

            var sql = @"
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

            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var multi = await connection.QueryMultipleAsync(
                sql,
                new
                {
                    term = fullTextTerm,
                    offset,
                    pageReq.PageSize
                }
            );

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
    }
}
