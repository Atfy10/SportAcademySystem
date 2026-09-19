namespace SportAcademy.Application.Interfaces;

/// <summary>
/// Resolves a user's contact destination for a channel. Precedence:
/// Employee's own contact info, else Trainee's, else the Identity login email (last resort) -
/// most users are exactly one of Employee/Trainee, but a login always has an Identity email.
/// </summary>
public interface IContactResolver
{
    Task<string?> ResolveEmailAsync(Guid userId, CancellationToken ct = default);
    Task<string?> ResolvePhoneAsync(Guid userId, CancellationToken ct = default);
}
