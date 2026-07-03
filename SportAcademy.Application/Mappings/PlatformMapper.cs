using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Application.Mappings;

public static class PlatformMapper
{
    public static TenantListResponse ToListResponse(this Tenant entity, int branchCount = 0, int userCount = 0)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Slug = entity.Slug,
            Code = entity.Code,
            Email = entity.Email,
            Status = entity.Status.ToString(),
            PlanName = entity.Subscription?.Plan?.Name,
            CreatedAt = entity.CreatedAt,
            BranchCount = branchCount,
            UserCount = userCount
        };

    public static TenantDetailResponse ToDetailResponse(this Tenant entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Slug = entity.Slug,
            Code = entity.Code,
            Email = entity.Email,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            OwnerId = entity.OwnerId,
            Profile = entity.Profile is not null ? new TenantProfileResponse
            {
                OrganizationName = entity.Profile.OrganizationName,
                LogoUrl = entity.Profile.LogoUrl,
                Phone = entity.Profile.Phone,
                Address = entity.Profile.Address,
                Website = entity.Profile.Website,
                Description = entity.Profile.Description
            } : null,
            Settings = entity.Settings is not null ? new TenantSettingsResponse
            {
                TimeZone = entity.Settings.TimeZone,
                Language = entity.Settings.Language,
                DateFormat = entity.Settings.DateFormat,
                TimeFormat = entity.Settings.TimeFormat,
                Currency = entity.Settings.Currency
            } : null,
            Subscription = entity.Subscription is not null ? new TenantSubscriptionResponse
            {
                PlanName = entity.Subscription.Plan?.Name ?? "",
                PlanCode = entity.Subscription.Plan?.Code ?? "",
                StartsAt = entity.Subscription.StartsAt,
                EndsAt = entity.Subscription.EndsAt,
                IsTrial = entity.Subscription.IsTrial,
                AutoRenew = entity.Subscription.AutoRenew
            } : null,
            Features = entity.Features?.Select(f => f.ToFeatureResponse()).ToList() ?? []
        };

    public static TenantFeatureResponse ToFeatureResponse(this TenantFeature feature)
        => new()
        {
            FeatureId = feature.FeatureId,
            Name = feature.Feature?.Name ?? "",
            DisplayName = feature.Feature?.DisplayName ?? "",
            Description = feature.Feature?.Description,
            IsEnabled = feature.IsEnabled,
            EnabledAt = feature.EnabledAt
        };

    public static PlatformDashboardResponse ToDashboardResponse(
        int totalTenants,
        Dictionary<string, int> statusCounts,
        int totalUsers,
        int totalBranches)
        => new()
        {
            TotalTenants = totalTenants,
            ActiveCount = statusCounts.GetValueOrDefault("Active", 0),
            PendingCount = statusCounts.GetValueOrDefault("PendingSetup", 0),
            SuspendedCount = statusCounts.GetValueOrDefault("Suspended", 0),
            ArchivedCount = statusCounts.GetValueOrDefault("Archived", 0),
            TotalUsers = totalUsers,
            TotalBranches = totalBranches
        };
}
