namespace SportAcademy.Application.DTOs.PublicDtos;

// One "block" in the public marketing site's bundle-builder game (/pricing/build) - and also
// what the SuperAdmin's bundle-pricing editor lists, since there's nothing sensitive here to
// hide from an anonymous visitor (the whole point is that a visitor sees exactly what a
// SuperAdmin priced). DirectPrerequisites/DirectDependents mirror FeatureDependencies.cs's real
// Requires graph exactly (code-defined, not DB-backed) - the client walks these itself to
// auto-complete a selection ("picking this block also picks/locks these"), the same closure
// FeatureDependencyPolicy already enforces server-side for real feature toggles.
public record BundleFeatureDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Description { get; init; }
    public string Category { get; init; } = default!;
    public decimal BundlePrice { get; init; }
    public bool IsCore { get; init; }
    public List<string> DirectPrerequisites { get; init; } = [];
    public List<string> DirectDependents { get; init; } = [];
}
