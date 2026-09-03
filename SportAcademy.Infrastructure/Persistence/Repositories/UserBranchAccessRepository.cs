using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class UserBranchAccessRepository : IUserBranchAccessRepository
    {
        private readonly ApplicationDbContext _context;

        public UserBranchAccessRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserBranchAccess>> GetForUserAsync(Guid userId, CancellationToken ct = default)
            => await _context.UserBranchAccesses
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task AddRangeAsync(IEnumerable<UserBranchAccess> access, CancellationToken ct = default)
        {
            await _context.UserBranchAccesses.AddRangeAsync(access, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ReplaceForUserAsync(Guid userId, Guid tenantId,
            IReadOnlyCollection<UserBranchAccess> access, CancellationToken ct = default)
        {
            var existing = await _context.UserBranchAccesses
                .Where(a => a.UserId == userId)
                .ToListAsync(ct);

            _context.UserBranchAccesses.RemoveRange(existing);

            foreach (var a in access)
            {
                a.UserId = userId;
                a.TenantId = tenantId;
            }

            await _context.UserBranchAccesses.AddRangeAsync(access, ct);

            await _context.SaveChangesAsync(ct);
        }
    }
}
