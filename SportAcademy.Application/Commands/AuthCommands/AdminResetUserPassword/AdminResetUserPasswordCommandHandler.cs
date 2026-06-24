using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;

public class AdminResetUserPasswordCommandHandler : IRequestHandler<AdminResetUserPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public AdminResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IUserContextService userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(Guid.Parse(_userContext.UserId), cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), _userContext.UserId);

        var passwordValid = await _userRepository.CheckPasswordAsync(admin, request.AdminPassword);
        if (!passwordValid)
        {
            return Result<bool>.Failure(
                _operation,
                "Admin password is incorrect.",
                400);
        }

        var targetUser = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        var identityResult = await _userRepository.AdminResetPasswordAsync(targetUser, request.NewPassword);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<bool>.Failure(
                _operation,
                "Failed to reset password.",
                400,
                errors);
        }

        return Result<bool>.Success(true, _operation);
    }
}
