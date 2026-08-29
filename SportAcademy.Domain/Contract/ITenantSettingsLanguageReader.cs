namespace SportAcademy.Domain.Contract
{
    /// <summary>
    /// Reads the current tenant's configured UI language.
    /// </summary>
    /// <remarks>
    /// A narrow seam so the culture middleware can consult tenant settings without the Web layer
    /// taking a dependency on EF or on a repository built for something else.
    /// </remarks>
    public interface ITenantSettingsLanguageReader
    {
        /// <summary>Returns the tenant's language, or null when there is no tenant or no setting.</summary>
        Task<string?> GetLanguageAsync(CancellationToken cancellationToken = default);
    }
}
