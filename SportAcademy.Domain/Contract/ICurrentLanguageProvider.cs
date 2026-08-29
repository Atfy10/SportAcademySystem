namespace SportAcademy.Domain.Contract
{
    /// <summary>
    /// Supplies the language for the current request.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="ITenantIdProvider"/>: an <c>AsyncLocal</c>-backed scoped service set by
    /// middleware, so anything downstream (localization, translated projections) can read the
    /// language without threading it through every call signature.
    /// </remarks>
    public interface ICurrentLanguageProvider
    {
        /// <summary>Neutral two-letter code, e.g. <c>en</c> or <c>ar</c>. Never null.</summary>
        string Language { get; }

        void SetLanguage(string? language);
    }
}
