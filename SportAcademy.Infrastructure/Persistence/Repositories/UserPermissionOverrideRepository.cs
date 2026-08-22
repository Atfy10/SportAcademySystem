using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class UserPermissionOverrideRepository : IUserPermissionOverrideRepository
    {
        private readonly ApplicationDbContext _context;

        public UserPermissionOverrideRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserPermissionOverride>> GetForUserAsync(Guid userId, CancellationToken ct = default)
            => await _context.UserPermissionOverrides
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task AddRangeAsync(IEnumerable<UserPermissionOverride> overrides, CancellationToken ct = default)
        {
            await _context.UserPermissionOverrides.AddRangeAsync(overrides, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ReplaceForUserAsync(Guid userId, Guid tenantId,
            IReadOnlyCollection<UserPermissionOverride> overrides, CancellationToken ct = default)
        {
            var existing = await _context.UserPermissionOverrides
                .Where(o => o.UserId == userId)
                .ToListAsync(ct);

            _context.UserPermissionOverrides.RemoveRange(existing);

            foreach (var o in overrides)
            {
                o.UserId = userId;
                o.TenantId = tenantId;
            }

            await _context.UserPermissionOverrides.AddRangeAsync(overrides, ct);

            await _context.SaveChangesAsync(ct);
        }
    }
}
