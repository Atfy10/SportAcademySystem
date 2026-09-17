namespace SportAcademy.Application.Interfaces;

/// <summary>
/// Normalizes a phone number to E.164 (e.g. "+201001234567") using the current tenant's
/// configured country, for storage. Presentation-layer formatting (spaces, local leading
/// zeros, whether the user typed a country code at all) must never reach the database as-is -
/// this is the single point that turns "whatever the client sent" into the one canonical form
/// every stored phone number takes, regardless of which tenant/country entered it.
/// </summary>
public interface IPhoneNumberNormalizer
{
    /// <summary>Returns the E.164 form of <paramref name="phone"/>, or the original value
    /// unchanged if it's null/blank or doesn't parse (format validity is FluentValidation's
    /// job, already enforced before a handler ever calls this - this never throws and never
    /// discards data on a parse hiccup).</summary>
    Task<string?> NormalizeAsync(string? phone, CancellationToken cancellationToken = default);
}
