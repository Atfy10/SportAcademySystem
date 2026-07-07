using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) => _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _context.Set<Tenant>().FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public Task<Tenant?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Set<Tenant>()
            .Include(t => t.Profile)
            .Include(t => t.Settings)
            .Include(t => t.Subscription).ThenInclude(s => s.Plan)
            .Include(t => t.Features).ThenInclude(f => f.Feature)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(List<Tenant> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, string? status, string? search, CancellationToken ct = default)
    {
        var query = _context.Set<Tenant>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, ignoreCase: true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.DisplayName.ToLower().Contains(term) ||
                t.Code.ToLower().Contains(term) ||
                t.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Subscription)
                .ThenInclude(s => s.Plan)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<int> GetCountAsync(CancellationToken ct = default)
        => _context.Set<Tenant>().CountAsync(ct);

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken ct = default)
    {
        var counts = await _context.Set<Tenant>()
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(c => c.Status, c => c.Count);
    }

    public Task<int> GetTotalUsersAsync(CancellationToken ct = default)
        => _context.Set<AppUser>().IgnoreQueryFilters().CountAsync(ct);

    public Task<int> GetTotalBranchesAsync(CancellationToken ct = default)
        => _context.Set<Branch>().IgnoreQueryFilters().CountAsync(ct);

    public Task<int> GetUserCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<AppUser>().CountAsync(u => u.TenantId == tenantId, ct);

    public Task<int> GetBranchCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<Branch>().CountAsync(b => b.TenantId == tenantId, ct);

    public Task<int> GetSportCountByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Set<Sport>().CountAsync(s => s.TenantId == tenantId, ct);

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Set<Tenant>().Where(t => t.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Set<Tenant>().Where(t => t.Code == code);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await _context.Set<Tenant>().AddAsync(tenant, ct);

    public Task<TenantFeature?> GetTenantFeatureAsync(Guid tenantId, Guid featureId, CancellationToken ct = default)
        => _context.Set<TenantFeature>()
            .FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FeatureId == featureId, ct);

    public async Task<List<TenantFeature>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Set<TenantFeature>()
            .Include(tf => tf.Feature)
            .Where(tf => tf.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task AddTenantFeatureAsync(TenantFeature feature, CancellationToken ct = default)
        => await _context.Set<TenantFeature>().AddAsync(feature, ct);

    public Task<List<Feature>> GetAllFeaturesAsync(CancellationToken ct = default)
        => _context.Set<Feature>().ToListAsync(ct);
}
