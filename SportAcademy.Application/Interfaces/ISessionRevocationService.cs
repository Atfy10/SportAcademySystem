using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

// Signs a user out of every device at once. Called by every write that changes what a user's
// existing session is allowed to be - their password, their roles, their permission overrides,
// their branch access, or whether they are allowed in at all - so none of those changes can be
// outlived by a session issued before it. Three things happen, each covering a different way a
// session stays alive:
//   1. the user's SecurityStamp rotates, so every access token already issued (they carry the
//      stamp as the "sstamp" claim) is rejected on its very next request, not after it expires;
//   2. every refresh token is revoked, so the client cannot simply mint a fresh access token;
//   3. a SessionRevoked push tells every open tab to sign out now, without waiting for its next
//      API call to fail.
public interface ISessionRevocationService
{
    Task RevokeAllSessionsAsync(AppUser user, string reason, CancellationToken ct = default);
}

/// The reason sent to the client with the SessionRevoked push, so the login page can tell the
/// user why they were signed out. Plain strings over the wire - the frontend matches on them.
public static class SessionRevocationReasons
{
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordReset = "PasswordReset";
    public const string RolesChanged = "RolesChanged";
    public const string PermissionsChanged = "PermissionsChanged";
    public const string AccountDeactivated = "AccountDeactivated";
}
