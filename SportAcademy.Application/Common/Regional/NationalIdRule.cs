using System.Text.RegularExpressions;

namespace SportAcademy.Application.Common.Regional;

/// <summary>
/// A country's National ID / civil ID format rule. Length and pattern are always checked;
/// <see cref="ChecksumValidator"/> is opt-in for the (currently one) country whose ID embeds a
/// verifiable structure beyond "N digits" - every other country here is length/pattern only.
/// </summary>
public sealed class NationalIdRule
{
    public required int? FixedLength { get; init; }
    public required Regex Pattern { get; init; }

    /// <summary>False (the default) everywhere - National ID is optional today for every entity
    /// that carries it (Trainee, Employee), so a country rule never forces it to be present.</summary>
    public bool IsRequired { get; init; }

    public Func<string, DateOnly?, bool>? ChecksumValidator { get; init; }

    /// <summary>Empty/whitespace is valid unless <see cref="IsRequired"/> - callers that need a
    /// value present enforce that separately (NotEmpty), same as the current per-entity
    /// validators already do.</summary>
    public bool IsValid(string? value, DateOnly? birthDate)
    {
        if (string.IsNullOrWhiteSpace(value))
            return !IsRequired;

        if (FixedLength is { } length && value.Length != length)
            return false;

        if (!Pattern.IsMatch(value))
            return false;

        return ChecksumValidator is null || ChecksumValidator(value, birthDate);
    }
}
