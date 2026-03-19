using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleUserActiveCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        user.IsBanned = !user.IsBanned;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<bool>.Success(!user.IsBanned, _operation);
    }
}
