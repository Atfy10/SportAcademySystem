namespace SportAcademy.Domain.Contract
{
    /// <summary>
    /// Reads the current tenant's configured country (ISO 3166-1 alpha-2). Same narrow-seam
    /// shape as ITenantSettingsCurrencyReader/ITenantSettingsLanguageReader: lets an
    /// Application-layer validator consult tenant settings without taking a dependency on EF or
    /// on a repository built for something else.
    /// </summary>
    public interface ITenantSettingsCountryReader
    {
        /// <summary>Returns the tenant's country, or the registry default when there is no
        /// ambient tenant or no settings row yet - a validator should never hard-fail just
        /// because regional context couldn't be resolved.</summary>
        Task<string> GetCountryAsync(CancellationToken cancellationToken = default);
    }
}
