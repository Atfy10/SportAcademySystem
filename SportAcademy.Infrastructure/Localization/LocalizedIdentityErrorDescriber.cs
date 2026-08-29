using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Localization;

namespace SportAcademy.Infrastructure.Localization;

/// <summary>
/// Replaces ASP.NET Identity's built-in English error text with catalog-backed messages.
/// </summary>
/// <remarks>
/// Without this, Identity failures reach the user as raw framework English - and several call
/// sites join them with "; " and surface the lot in a toast.
/// </remarks>
public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly ILocalizationService _localizer;

    public LocalizedIdentityErrorDescriber(ILocalizationService localizer) => _localizer = localizer;

    private IdentityError Error(string code, params object[] args) =>
        new() { Code = code, Description = _localizer["identity." + code, args] };

    public override IdentityError DefaultError() => Error(nameof(DefaultError));

    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName));

    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail));

    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName));

    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail));

    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), length);

    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit));

    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower));

    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper));

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric));

    public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch));

    public override IdentityError UserAlreadyHasPassword() => Error(nameof(UserAlreadyHasPassword));

    public override IdentityError InvalidToken() => Error(nameof(InvalidToken));
}
