using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Repositories
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

        public async Task<bool> IsExistByIdAsync(int id, CancellationToken cancellationToken)
            => await _context.Sports
                .AnyAsync(s => s.Id == id, cancellationToken);

    }
}
