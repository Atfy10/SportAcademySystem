using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class CoachBranchAccessRepository : ICoachBranchAccessRepository
    {
        private readonly ApplicationDbContext _context;

        public CoachBranchAccessRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CoachBranchAccess>> GetForCoachAsync(int coachId, CancellationToken ct = default)
            => await _context.CoachBranchAccesses
                .Where(a => a.CoachId == coachId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task ReplaceForCoachAsync(int coachId, Guid tenantId,
            IReadOnlyCollection<CoachBranchAccess> access, CancellationToken ct = default)
        {
            var existing = await _context.CoachBranchAccesses
                .Where(a => a.CoachId == coachId)
                .ToListAsync(ct);

            _context.CoachBranchAccesses.RemoveRange(existing);

            foreach (var a in access)
            {
                a.CoachId = coachId;
                a.TenantId = tenantId;
            }

            await _context.CoachBranchAccesses.AddRangeAsync(access, ct);

            await _context.SaveChangesAsync(ct);
        }
    }
}
