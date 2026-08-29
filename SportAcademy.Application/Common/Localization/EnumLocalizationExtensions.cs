namespace SportAcademy.Application.Common.Localization
{
    /// <summary>
    /// Localized labels for enums that reach the UI as option lists.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>value</c> stays the English enum token, because it is what the database stores
    /// (many enums are persisted via <c>HasConversion&lt;string&gt;()</c>) and what raw SQL matches
    /// on, e.g. <c>sd.Status = N'Active'</c>. Only the <c>label</c> follows the request language.
    /// </para>
    /// <para>
    /// Backend-owned on purpose: the labels used to be duplicated between here and the frontend,
    /// which is how the <c>Cancelled</c>/<c>Canceled</c> drift between the two sides appeared.
    /// </para>
    /// </remarks>
    public static class EnumLocalizationExtensions
    {
        /// <summary>Resolves <c>enum.{TypeName}.{Member}</c>, falling back to the member name.</summary>
        public static string Label<TEnum>(this ILocalizationService localizer, TEnum value)
            where TEnum : struct, Enum
        {
            var key = $"enum.{typeof(TEnum).Name}.{value}";
            return localizer.Exists(key) ? localizer[key] : value.ToString();
        }

        /// <summary>Builds a <c>{ value, label }</c> option list for an entire enum.</summary>
        public static List<EnumOption> Options<TEnum>(
            this ILocalizationService localizer,
            Func<TEnum, bool>? filter = null)
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Where(v => filter is null || filter(v))
                .Select(v => new EnumOption(v.ToString(), localizer.Label(v)))
                .ToList();
        }
    }

    /// <param name="Value">English enum token - the wire and storage contract.</param>
    /// <param name="Label">Human-readable text in the request's language.</param>
    public record EnumOption(string Value, string Label);
}
