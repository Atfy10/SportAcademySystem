using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.VerifyPassword;

public class VerifyPasswordCommandHandler : IRequestHandler<VerifyPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.VerifyPassword.ToString();

    public VerifyPasswordCommandHandler(IUserRepository userRepository, IUserContextService userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(_userContext.UserId), cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), _userContext.UserId);

        var isValid = await _userRepository.CheckPasswordAsync(user, request.Password);
        if (!isValid)
            return Result<bool>.Failure(_operation, "Invalid password.", 400);

        return Result<bool>.Success(true, _operation);
    }
}
