namespace SportAcademy.Application.Common.Localization
{
    /// <summary>
    /// Resolves message keys to text in the current request's language.
    /// </summary>
    /// <remarks>
    /// Declared in Application rather than Infrastructure so pipeline behaviors and handlers can
    /// depend on it without inverting the layer dependencies.
    /// <para>
    /// Keys are dotted and mirror the frontend catalogs (<c>errors.trainee.notFound</c>), so a
    /// single translator glossary serves both sides.
    /// </para>
    /// </remarks>
    public interface ILocalizationService
    {
        /// <summary>
        /// Returns the localized string for <paramref name="key"/>, formatted with
        /// <paramref name="args"/>. Falls back to English and, failing that, to the key itself -
        /// a missing translation must never blank out a message.
        /// </summary>
        string this[string key, params object[] args] { get; }

        /// <summary>Resolves in an explicit language, ignoring the current request.</summary>
        string GetIn(string language, string key, params object[] args);

        /// <summary>True when the key exists in either catalog. Used to decide whether a legacy
        /// literal message should be kept instead of rendering a raw key.</summary>
        bool Exists(string key);
    }
}
