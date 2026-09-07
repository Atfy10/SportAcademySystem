namespace SportAcademy.Application.Common.Features;

// Single source of truth for which display category a feature's slug belongs to - shared by
// TenantQueries.GetTenantFeatures (the Owner's Settings page) and PlatformQueries.GetTenantFeatures
// (the SuperAdmin's per-tenant Features tab) so the two views group and count features identically.
// Before this was extracted, only the Owner-facing handler carried this map and the Platform one
// had no Category at all, forcing the SuperAdmin's page to fuzzy-match feature names against its
// own separately-maintained catalog just to approximate grouping - the two pages could show
// different groupings for the same tenant.
public static class FeatureCategories
{
    private static readonly Dictionary<string, string> Map = new()
    {
        ["user-management"] = "Management",
        ["role-management"] = "Management",
        ["tenant-settings"] = "Management",
        ["branch-management"] = "Management",
        ["trainee-management"] = "Management",
        ["employee-management"] = "Management",
        ["coach-management"] = "Management",
        ["sport-management"] = "Management",
        ["subscription-plan"] = "Management",
        ["pricing-management"] = "Management",
        ["payment-processing"] = "Management",
        ["profile-mgmt"] = "Management",
        ["api-access"] = "Management",
        ["backup-restore"] = "Management",
        ["system-settings"] = "Management",
        ["trainee-codes"] = "Management",
        ["group-management"] = "Operations",
        ["schedule-management"] = "Operations",
        ["attendance-tracking"] = "Operations",
        ["enrollment-management"] = "Operations",
        ["family-management"] = "Operations",
        ["nationality-categories"] = "Operations",
        ["session-management"] = "Operations",
        ["financial-reports"] = "Finance",
        ["discount-offers"] = "Finance",
        ["notifications"] = "Communication",
        ["chat-system"] = "Communication",
        ["trainee-reports"] = "Analytics",
        ["coach-reports"] = "Analytics",
        ["operational-reports"] = "Analytics",
        ["attendance-reports"] = "Analytics",
        ["video-analysis"] = "Analytics",
        ["health-test-mgmt"] = "Analytics",
        ["ai-assistant"] = "Analytics",
        ["audit-trail"] = "Security",
    };

    public static string For(string featureName) => Map.GetValueOrDefault(featureName, "Management");
}
