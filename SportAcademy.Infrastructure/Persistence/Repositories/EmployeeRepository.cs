using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee, int>, IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Employee?> GetFullEmployee(int id, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.Id == id)
                .Include(e => e.Branch)
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<bool> IsSSNExistAsync(string ssn, CancellationToken cancellationToken = default)
            => await _context.Employees
                .Where(e => e.SSN == ssn)
                .FirstOrDefaultAsync(cancellationToken) is not null;
        
    }
}
