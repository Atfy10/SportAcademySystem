using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public ResetPasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // Cross-tenant lookup: this endpoint is unauthenticated, so there is no ambient tenant
        // to filter by in the first place (the global query filter would match zero rows
        // otherwise, since an unset ambient tenant filters everything out, not everything in).
        var user = await _userRepository.GetByIdIgnoringTenantAsync(request.UserId, ct);
        if (user is null || user.IsBanned)
            return Result<bool>.Failure(_operation, "Invalid or expired reset link.", 400);

        var identityResult = await _userRepository.ConsumePasswordResetTokenAsync(user, request.Token, request.NewPassword);
        if (!identityResult.Succeeded)
            return Result<bool>.Failure(
                _operation,
                string.Join(" ", identityResult.Errors.Select(e => e.Description)),
                400);

        return Result<bool>.Success(true, _operation);
    }
}
