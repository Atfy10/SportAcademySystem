using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Services;

namespace SportAcademy.Web.Middleware
{
    /// <summary>
    /// Resolves the language for the request: Accept-Language header, then the authenticated
    /// user's tenant setting, then English.
    /// </summary>
    /// <remarks>
    /// Must be registered <em>after</em> <c>UseAuthentication()</c>. TenantResolutionMiddleware
    /// runs before authentication, so the tenant's configured language is not knowable there -
    /// only the header would be. Placing this after authentication lets both sources work.
    /// </remarks>
    public class CultureResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public CultureResolutionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            ICurrentLanguageProvider languageProvider,
            ITenantSettingsLanguageReader tenantLanguage)
        {
            var language = ResolveFromHeader(context);

            // Only consult the tenant default when the client did not ask for a language.
            if (language is null && context.User?.Identity?.IsAuthenticated == true)
            {
                language = await tenantLanguage.GetLanguageAsync(context.RequestAborted);
            }

            languageProvider.SetLanguage(language);

            await _next(context);
        }

        /// <summary>
        /// Picks the highest-quality supported language from Accept-Language, ignoring entries we
        /// have no catalog for rather than falling over on the first unsupported one.
        /// </summary>
        private static string? ResolveFromHeader(HttpContext context)
        {
            var header = context.Request.Headers.AcceptLanguage.ToString();
            if (string.IsNullOrWhiteSpace(header)) return null;

            var best = header
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part =>
                {
                    var pieces = part.Split(';', StringSplitOptions.TrimEntries);
                    var tag = pieces[0];
                    var quality = 1.0d;

                    var q = pieces.FirstOrDefault(p => p.StartsWith("q=", StringComparison.OrdinalIgnoreCase));
                    if (q is not null && double.TryParse(q[2..], out var parsed)) quality = parsed;

                    return (Tag: tag, Quality: quality);
                })
                .Where(x => x.Quality > 0)
                .OrderByDescending(x => x.Quality)
                .Select(x => x.Tag)
                .FirstOrDefault(tag =>
                    CurrentLanguageProvider.Supported.Contains(
                        CurrentLanguageProvider.Normalize(tag), StringComparer.OrdinalIgnoreCase) &&
                    !string.Equals(tag, "*", StringComparison.Ordinal));

            return best is null ? null : CurrentLanguageProvider.Normalize(best);
        }
    }
}
