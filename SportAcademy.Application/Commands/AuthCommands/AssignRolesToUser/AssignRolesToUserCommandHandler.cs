using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionCacheInvalidator _cacheInvalidator;
    private readonly string _operation = OperationType.Update.ToString();

    public AssignRolesToUserCommandHandler(IUserRepository userRepository, IPermissionCacheInvalidator cacheInvalidator)
    {
        _userRepository = userRepository;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<Result<bool>> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        var (result, notFoundRoles) = await _userRepository.ReplaceRolesAsync(user, request.Roles, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<bool>.Failure(_operation, "Failed to assign roles.", 400, errors);
        }

        // A role change directly changes the resolved permission set - without this, a
        // just-demoted user would keep their old effective permissions for up to the cache's
        // sliding window.
        _cacheInvalidator.Invalidate(request.UserId);

        return Result<bool>.Success(true, _operation);
    }
}
