using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Security;
using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;

public class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Result<AdminCreateUserResultDto>>
{
    // Owner and SuperAdmin are deliberately excluded, same reasoning (and the same set) as
    // CreateInvitationCommandHandler.InvitableStaffRoles: Owner is unique per tenant and only
    // ever set via invitation acceptance, SuperAdmin is platform-only.
    private static readonly string[] AssignableRoles = ["Admin", "Employee", "Accountant"];

    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUserBranchAccessRepository _userBranchAccessRepository;
    private readonly string _operationType = OperationType.Add.ToString();

    public AdminCreateUserCommandHandler(
        IMapper mapper,
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IUserBranchAccessRepository userBranchAccessRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _userBranchAccessRepository = userBranchAccessRepository;
    }

    public async Task<Result<AdminCreateUserResultDto>> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validated before any write - a bad role/branch choice must never leave a role-less
        // account behind the way a failure partway through used to (see AdminCreateUserCommand's
        // own comment on why Role is required at all here).
        if (!AssignableRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"'{request.Role}' is not a role that can be assigned here.",
                400,
                new Dictionary<string, string[]> { ["Role"] = [$"'{request.Role}' is not a role that can be assigned here."] });
        }

        // Employee is the only branch-restricted role (see IBranchAccessProvider) - a new
        // Employee with zero branches would see and be able to write no branch-scoped data at
        // all, which is never what an admin actually wants, so it's rejected outright rather
        // than silently creating a locked-out account (same rule CreateInvitationCommandHandler
        // already enforces for an invited Employee).
        if (string.Equals(request.Role, "Employee", StringComparison.OrdinalIgnoreCase) &&
            request.BranchIds is not { Count: > 0 })
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                "At least one branch must be selected for an Employee.",
                400,
                new Dictionary<string, string[]> { ["BranchIds"] = ["At least one branch must be selected for an Employee."] });
        }

        if (await _userRepository.IsUsernameExistAsync(request.UserName, cancellationToken))
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"Username '{request.UserName}' is already taken.",
                400,
                new Dictionary<string, string[]>
                {
                    ["UserName"] = [$"Username '{request.UserName}' is already taken."]
                });
        }

        if (await _userRepository.IsEmailExistAsync(request.Email, cancellationToken))
        {
            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"Email '{request.Email}' is already registered.",
                400,
                new Dictionary<string, string[]>
                {
                    ["Email"] = [$"Email '{request.Email}' is already registered."]
                });
        }

        var user = _mapper.Map<AppUser>(request)
            ?? throw new AutoMapperMappingException("Error occurred while mapping.");

        user.IsBanned = !request.IsActive;
        user.EmailConfirmed = request.EmailConfirmed;

        var password = SecurePasswordGenerator.Generate();

        var identityResult = await _userRepository.Register(user, password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                "Failed to create user.",
                400,
                errors);
        }

        var profile = new Domain.Entities.Profile
        {
            AppUserId = user.Id,
        };

        await _profileRepository.AddAsync(profile, cancellationToken);

        var roleResult = await _userRepository.AssignToRole(user, request.Role);
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Result<AdminCreateUserResultDto>.Failure(
                _operationType,
                $"User was created, but assigning the '{request.Role}' role failed.",
                400,
                errors);
        }

        if (string.Equals(request.Role, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            var branchAccess = request.BranchIds!.Select(branchId => new UserBranchAccess
            {
                UserId = user.Id,
                TenantId = user.TenantId,
                BranchId = branchId,
            });
            await _userBranchAccessRepository.AddRangeAsync(branchAccess, cancellationToken);
        }

        return Result<AdminCreateUserResultDto>.Success(
            new AdminCreateUserResultDto
            {
                UserId = user.Id,
                UserName = user.UserName!,
                GeneratedPassword = password
            },
            _operationType);
    }
}
