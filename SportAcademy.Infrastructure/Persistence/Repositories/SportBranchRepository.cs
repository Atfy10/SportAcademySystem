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
    public class SportBranchRepository : BaseRepository<SportBranch, int>, ISportBranchRepository
    {
        private readonly ApplicationDbContext _context;

        public SportBranchRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsExistAsync(int sportId, int branchId, CancellationToken cancellationToken)
            => await _context.SportBranchs
                .AnyAsync(sb => sb.SportId == sportId && sb.BranchId == branchId, cancellationToken);
    }
}
