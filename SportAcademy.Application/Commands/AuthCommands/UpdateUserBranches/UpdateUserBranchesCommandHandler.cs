using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.AuthCommands.UpdateUserBranches;

public class UpdateUserBranchesCommandHandler : IRequestHandler<UpdateUserBranchesCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBranchAccessRepository _userBranchAccessRepository;
    private readonly IUserContextService _userContext;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateUserBranchesCommandHandler(
        IUserRepository userRepository,
        IUserBranchAccessRepository userBranchAccessRepository,
        IUserContextService userContext,
        IPublisher publisher)
    {
        _userRepository = userRepository;
        _userBranchAccessRepository = userBranchAccessRepository;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(UpdateUserBranchesCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

        var roles = await _userRepository.GetUserRoleAsync(user, ct);
        if (!roles.Contains("Employee", StringComparer.OrdinalIgnoreCase))
            return Result<bool>.Failure(
                _operation, "Branch access only applies to the Employee role.", 400);

        var access = request.BranchIds
            .Distinct()
            .Select(branchId => new UserBranchAccess { BranchId = branchId });

        await _userBranchAccessRepository.ReplaceForUserAsync(user.Id, user.TenantId, access.ToList(), ct);

        var actorName = _userContext.UserId is { } actorId
            ? await _userRepository.GetDisplayNameAsync(actorId, ct)
            : "System";
        await _publisher.Publish(new UserBranchesChangedEvent(user.Id, actorName), ct);

        return Result<bool>.Success(true, _operation);
    }
}
