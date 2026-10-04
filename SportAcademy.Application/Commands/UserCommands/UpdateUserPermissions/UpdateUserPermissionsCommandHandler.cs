using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions
{
    public class UpdateUserPermissionsCommandHandler : IRequestHandler<UpdateUserPermissionsCommand, Result<bool>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserPermissionOverrideRepository _overrideRepository;
        private readonly IPermissionCacheInvalidator _cacheInvalidator;
        private readonly ISessionRevocationService _sessionRevocation;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateUserPermissionsCommandHandler(
            UserManager<AppUser> userManager,
            IUserPermissionOverrideRepository overrideRepository,
            IPermissionCacheInvalidator cacheInvalidator,
            ISessionRevocationService sessionRevocation)
        {
            _userManager = userManager;
            _overrideRepository = overrideRepository;
            _cacheInvalidator = cacheInvalidator;
            _sessionRevocation = sessionRevocation;
        }

        public async Task<Result<bool>> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

            var invalid = request.Overrides
                .Where(o => !Permissions.All.Contains(o.Permission) || o.Permission.StartsWith("platform."))
                .Select(o => o.Permission)
                .ToList();
            if (invalid.Count > 0)
                return Result<bool>.Failure(_operation, $"Unknown or unassignable permission(s): {string.Join(", ", invalid)}", 400);

            var overrides = request.Overrides
                .DistinctBy(o => o.Permission)
                .Select(o => new UserPermissionOverride { Permission = o.Permission, Effect = o.Effect })
                .ToList();

            // Re-saving an unchanged set from the Users & Roles page must not sign the user out.
            var existing = await _overrideRepository.GetForUserAsync(user.Id, cancellationToken);
            var overridesChanged = !existing
                .Select(o => (o.Permission, o.Effect))
                .ToHashSet()
                .SetEquals(overrides.Select(o => (o.Permission, o.Effect)));

            await _overrideRepository.ReplaceForUserAsync(user.Id, user.TenantId, overrides, cancellationToken);

            _cacheInvalidator.Invalidate(user.Id);

            if (overridesChanged)
                await _sessionRevocation.RevokeAllSessionsAsync(user, SessionRevocationReasons.PermissionsChanged, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
