using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;

public class AdminResetUserPasswordCommandHandler : IRequestHandler<AdminResetUserPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public AdminResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IUserContextService userContext,
        IPublisher publisher)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        if (userId is null)
            return Result<bool>.Failure(_operation, "User ID is not available in the context.", 400);

        var admin = await _userRepository.GetByIdAsync(userId.Value, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), userId);

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

        var actorName = await _userRepository.GetDisplayNameAsync(admin.Id, cancellationToken);
        await _publisher.Publish(new PasswordResetByAdminEvent(targetUser.Id, actorName), cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
