namespace SportAcademy.Domain.Contract
{
    /// <summary>
    /// Reads the current tenant's configured currency.
    /// </summary>
    /// <remarks>
    /// Same narrow-seam shape as ITenantSettingsLanguageReader: lets an Application-layer money-
    /// creation path (Payment/Invoice) consult tenant settings without taking a dependency on EF
    /// or on a repository built for something else.
    /// </remarks>
    public interface ITenantSettingsCurrencyReader
    {
        /// <summary>Returns the tenant's currency, or null when there is no tenant or no setting.</summary>
        Task<string?> GetCurrencyAsync(CancellationToken cancellationToken = default);
    }
}
