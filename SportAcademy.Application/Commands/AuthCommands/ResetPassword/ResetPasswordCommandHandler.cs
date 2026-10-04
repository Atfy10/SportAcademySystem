using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly ISessionRevocationService _sessionRevocation;
    private readonly string _operation = OperationType.Update.ToString();

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        ITenantIdProvider tenantIdProvider,
        ISessionRevocationService sessionRevocation)
    {
        _userRepository = userRepository;
        _tenantIdProvider = tenantIdProvider;
        _sessionRevocation = sessionRevocation;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // Cross-tenant lookup: this endpoint is unauthenticated, so there is no ambient tenant
        // to filter by in the first place (the global query filter would match zero rows
        // otherwise, since an unset ambient tenant filters everything out, not everything in).
        var user = await _userRepository.GetByIdIgnoringTenantAsync(request.UserId, ct);
        if (user is null || user.IsBanned)
            return Result<bool>.Failure(_operation, "Invalid or expired reset link.", 400);

        // The same missing ambient tenant would make TenantSaveChangesInterceptor refuse the
        // password write itself (AppUser is ITenantScoped) - run it under the user's own tenant.
        IdentityResult identityResult;
        using (_tenantIdProvider.Impersonate(user.TenantId))
        {
            identityResult = await _userRepository.ConsumePasswordResetTokenAsync(user, request.Token, request.NewPassword);
        }
        if (!identityResult.Succeeded)
            return Result<bool>.Failure(
                _operation,
                string.Join(" ", identityResult.Errors.Select(e => e.Description)),
                400);

        // Whoever held the old password - possibly the reason for the reset - loses every session.
        await _sessionRevocation.RevokeAllSessionsAsync(user, SessionRevocationReasons.PasswordChanged, ct);

        return Result<bool>.Success(true, _operation);
    }
}
