using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System.Security.Claims;

namespace SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions
{
    public class UpdateUserPermissionsCommandHandler : IRequestHandler<UpdateUserPermissionsCommand, Result<bool>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateUserPermissionsCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new IdNotFoundException(nameof(AppUser), request.UserId);

            var invalid = request.Permissions.Where(p => !Permissions.All.Contains(p)).ToList();
            if (invalid.Count > 0)
                return Result<bool>.Failure(_operation, $"Unknown permission(s): {string.Join(", ", invalid)}", 400);

            var existingClaims = await _userManager.GetClaimsAsync(user);
            var existingPermissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();

            if (existingPermissionClaims.Count > 0)
            {
                var removeResult = await _userManager.RemoveClaimsAsync(user, existingPermissionClaims);
                if (!removeResult.Succeeded)
                    return Result<bool>.Failure(_operation, "Failed to update permissions.", 400);
            }

            var newClaims = request.Permissions.Distinct().Select(p => new Claim("permission", p)).ToList();
            if (newClaims.Count > 0)
            {
                var addResult = await _userManager.AddClaimsAsync(user, newClaims);
                if (!addResult.Succeeded)
                    return Result<bool>.Failure(_operation, "Failed to update permissions.", 400);
            }

            return Result<bool>.Success(true, _operation);
        }
    }
}
