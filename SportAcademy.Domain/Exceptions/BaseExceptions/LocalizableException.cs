namespace SportAcademy.Domain.Exceptions.BaseExceptions
{
    /// <summary>
    /// Base for domain exceptions that carry a message <em>key</em> plus arguments rather than a
    /// finished English sentence.
    /// </summary>
    /// <remarks>
    /// The Domain layer stays dependency-free - there is no localizer here. The key is resolved at
    /// the HTTP boundary by <c>ExceptionHandlingBehavior</c>, which knows the request's language.
    /// <para>
    /// <see cref="Exception.Message"/> is still populated with the English fallback so that
    /// logging, tests and any un-migrated code path keep working unchanged. That is what makes the
    /// migration incremental: exceptions can adopt this base a few at a time, and anything not yet
    /// migrated simply keeps emitting its literal English.
    /// </para>
    /// </remarks>
    public abstract class LocalizableException : Exception
    {
        protected LocalizableException(string messageKey, string fallbackMessage, params object[] args)
            : base(fallbackMessage)
        {
            MessageKey = messageKey;
            Args = args ?? [];
        }

        /// <summary>Dotted catalog key, e.g. <c>errors.enrollment.groupSportMismatch</c>.</summary>
        public string MessageKey { get; }

        /// <summary>Interpolation arguments for the localized template.</summary>
        public object[] Args { get; }
    }
}
