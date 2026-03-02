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
    public class SportRepository : BaseRepository<Sport, int>, ISportRepository
    {
        private readonly ApplicationDbContext _context;

        public SportRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sport>> GetAvailableSportsForBranch(int branchId, CancellationToken cancellationToken)
            => await _context.Sports
                .Where(s => !s.Branches.Any(sb => sb.SportId == s.Id && sb.BranchId == branchId))
                .ToListAsync(cancellationToken);

        public async Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default)
           => await _context.Sports
               .AnyAsync(s => s.Name == name, cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Sports.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<string>> SearchAsync(string term, CancellationToken cancellationToken = default)
        {
            var pattern = $"%{term}%";

            return await _context.Sports
                .Select(s => s.Name)
                .Where(s => EF.Functions.Like(s, pattern))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

    }
}
