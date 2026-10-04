using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly ISessionRevocationService _sessionRevocation;
    private readonly string _operation = OperationType.Update.ToString();

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUserContextService userContext,
        ISessionRevocationService sessionRevocation)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _sessionRevocation = sessionRevocation;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        if (userId is null)
            return Result<bool>.Failure(_operation, "User ID is not available in the context.", 400);

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

        var result = await _userRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return Result<bool>.Failure(_operation,
                "Failed to change password.", 400, errors);
        }

        // Every device is signed out, this one included - the client sends the user back to the
        // login page to sign in with the new password.
        await _sessionRevocation.RevokeAllSessionsAsync(user, SessionRevocationReasons.PasswordChanged, cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
