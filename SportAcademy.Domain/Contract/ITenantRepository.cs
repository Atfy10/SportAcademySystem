using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Contract;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetDetailByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Tenant> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, string? status, string? search, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken ct = default);
    Task<int> GetTotalUsersAsync(CancellationToken ct = default);
    Task<int> GetTotalBranchesAsync(CancellationToken ct = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<TenantFeature?> GetTenantFeatureAsync(Guid tenantId, Guid featureId, CancellationToken ct = default);
    Task<List<TenantFeature>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default);
    Task AddTenantFeatureAsync(TenantFeature feature, CancellationToken ct = default);
    Task<List<Domain.Entities.Feature>> GetAllFeaturesAsync(CancellationToken ct = default);
}
