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
    public class SportBranchRepository : ISportBranchRepository
    {
        private readonly ApplicationDbContext _context;

        public SportBranchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int sportId, int branchId, CancellationToken cancellationToken)
        {
            return await _context.SportBranchs
                .AnyAsync(sb => sb.SportId == sportId && sb.BranchId == branchId, cancellationToken);
        }

        public async Task AddAsync(SportBranch entity, CancellationToken cancellationToken)
        {
            await _context.SportBranchs.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
