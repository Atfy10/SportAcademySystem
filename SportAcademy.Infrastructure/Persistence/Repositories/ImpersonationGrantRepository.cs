using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ImpersonationGrantRepository : IImpersonationGrantRepository
    {
        private readonly ApplicationDbContext _context;

        public ImpersonationGrantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TenantImpersonationGrant grant, CancellationToken ct = default)
        {
            await _context.Set<TenantImpersonationGrant>().AddAsync(grant, ct);
            await _context.SaveChangesAsync(ct);
        }

        public Task<TenantImpersonationGrant?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _context.Set<TenantImpersonationGrant>().FirstOrDefaultAsync(g => g.Id == id, ct);

        public async Task UpdateAsync(TenantImpersonationGrant grant, CancellationToken ct = default)
        {
            _context.Set<TenantImpersonationGrant>().Update(grant);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<PagedData<TenantImpersonationGrantDto>> GetPagedForTenantAsync(
            Guid tenantId, PageRequest page, CancellationToken ct = default)
        {
            var query = _context.Set<TenantImpersonationGrant>()
                .AsNoTracking()
                .Where(g => g.TenantId == tenantId)
                .OrderByDescending(g => g.StartedAt);

            var totalCount = await query.CountAsync(ct);
            var grants = await query
                .Skip(page.Skip)
                .Take(page.PageSize)
                .ToListAsync(ct);

            // The granting user is a SuperAdmin, whose own tenant is System - the ambient tenant
            // context here is the CALLER's (the tenant Owner asking "who accessed my account"),
            // so the normal tenant query filter would silently exclude that user. Same
            // cross-tenant lookup UserRepository.GetOwnerByIdAsync/GetByIdIgnoringTenantAsync use.
            var grantedByIds = grants.Select(g => g.GrantedByUserId).Distinct().ToList();
            var names = await _context.AppUsers
                .IgnoreQueryFilters()
                .Where(u => grantedByIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Unknown", ct);

            var now = DateTime.UtcNow;
            var items = grants.Select(g => new TenantImpersonationGrantDto(
                g.Id,
                g.TenantId,
                names.GetValueOrDefault(g.GrantedByUserId, "Unknown"),
                g.Reason,
                g.StartedAt,
                g.ExpiresAt,
                g.EndedAt,
                g.EndedReason,
                g.EndedAt is null && g.ExpiresAt > now
            )).ToList();

            return new PagedData<TenantImpersonationGrantDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize,
            };
        }
    }
}
