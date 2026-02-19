using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

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

        public Task<PagedData<EmployeeDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.Employees
                .AsNoTracking()
                .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
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
    }
}
