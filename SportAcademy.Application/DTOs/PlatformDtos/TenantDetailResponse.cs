namespace SportAcademy.Application.DTOs.PlatformDtos;

public record TenantDetailResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public Guid? OwnerId { get; init; }

    public int UserCount { get; init; }
    public int BranchCount { get; init; }
    public int SportCount { get; init; }

    public TenantProfileResponse? Profile { get; init; }
    public TenantSettingsResponse? Settings { get; init; }
    public TenantSubscriptionResponse? Subscription { get; init; }
    public List<TenantFeatureResponse> Features { get; init; } = [];
}

public record TenantProfileResponse
{
    public string OrganizationName { get; init; } = default!;
    public string? LogoUrl { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }
    public string? Description { get; init; }
}

public record TenantSettingsResponse
{
    public string TimeZone { get; init; } = default!;
    public string Language { get; init; } = default!;
    public string DateFormat { get; init; } = default!;
    public string TimeFormat { get; init; } = default!;
    public string Currency { get; init; } = default!;
}

public record TenantSubscriptionResponse
{
    public string PlanName { get; init; } = default!;
    public string PlanCode { get; init; } = default!;
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public bool IsTrial { get; init; }
    public bool AutoRenew { get; init; }
}
