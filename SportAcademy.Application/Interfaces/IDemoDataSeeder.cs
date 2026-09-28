namespace SportAcademy.Application.Interfaces
{
    public enum DemoSeedOutcome
    {
        /// <summary>The demo tenant did not exist and has just been created.</summary>
        Seeded,

        /// <summary>The demo tenant already exists; nothing was changed.</summary>
        AlreadySeeded,
    }

    /// <param name="Outcome">What happened.</param>
    /// <param name="TenantId">The demo tenant's id (created now, or found).</param>
    /// <param name="TenantSlug">Slug to log in at, <c>/t/{slug}/login</c>.</param>
    /// <param name="OwnerUserName">The demo Owner's user name. The password is intentionally not
    /// returned - it is a development default that lives in the seeder.</param>
    public sealed record DemoSeedResult(
        DemoSeedOutcome Outcome,
        Guid? TenantId = null,
        string? TenantSlug = null,
        string? OwnerUserName = null);

    /// <summary>
    /// Creates the fictional demo tenant with a full business dataset. Environment gating is the
    /// caller's job (see SeedDemoDataCommandHandler) - this only decides whether there is
    /// anything left to seed.
    /// </summary>
    public interface IDemoDataSeeder
    {
        Task<DemoSeedResult> SeedAsync(CancellationToken ct = default);
    }
}
