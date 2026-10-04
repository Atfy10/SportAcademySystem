using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;
using SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;
using SportAcademy.Application.Commands.AuthCommands.ChangePassword;
using SportAcademy.Application.Commands.AuthCommands.ResetPassword;
using SportAcademy.Application.Commands.AuthCommands.UpdateUserBranches;
using SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

// Every write that changes what a user's existing session may do must sign that user out of
// every device (ISessionRevocationService) - and re-saving an unchanged form must not.
public class SessionRevocationTriggerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ISessionRevocationService> _sessionRevocationMock = new();

    private readonly AppUser _user = new()
    {
        Id = Guid.NewGuid(),
        TenantId = Guid.NewGuid(),
        UserName = "target",
        Email = "target@test.com",
    };

    public SessionRevocationTriggerTests()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _userRepoMock.Setup(r => r.GetDisplayNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync("Actor");
    }

    private void VerifyRevoked(string reason) =>
        _sessionRevocationMock.Verify(s => s.RevokeAllSessionsAsync(_user, reason, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyNotRevoked() =>
        _sessionRevocationMock.Verify(s => s.RevokeAllSessionsAsync(
            It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

    // ── Passwords ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_Success_RevokesAllSessions()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_user.Id);
        _userRepoMock.Setup(r => r.ChangePasswordAsync(_user, "Old#1234", "New#1234")).ReturnsAsync(IdentityResult.Success);
        var handler = new ChangePasswordCommandHandler(_userRepoMock.Object, _userContextMock.Object, _sessionRevocationMock.Object);

        var result = await handler.Handle(new ChangePasswordCommand("Old#1234", "New#1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        VerifyRevoked(SessionRevocationReasons.PasswordChanged);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_RevokesNothing()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_user.Id);
        _userRepoMock.Setup(r => r.ChangePasswordAsync(_user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }));
        var handler = new ChangePasswordCommandHandler(_userRepoMock.Object, _userContextMock.Object, _sessionRevocationMock.Object);

        var result = await handler.Handle(new ChangePasswordCommand("wrong", "New#1234"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        VerifyNotRevoked();
    }

    [Fact]
    public async Task ResetPasswordLink_Success_RevokesAllSessions_UnderTheUsersTenant()
    {
        var tenantIdProviderMock = new Mock<ITenantIdProvider>();
        tenantIdProviderMock.Setup(p => p.Impersonate(It.IsAny<Guid>())).Returns(Mock.Of<IDisposable>());
        _userRepoMock.Setup(r => r.GetByIdIgnoringTenantAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _userRepoMock.Setup(r => r.ConsumePasswordResetTokenAsync(_user, "token", "New#1234")).ReturnsAsync(IdentityResult.Success);
        var handler = new ResetPasswordCommandHandler(
            _userRepoMock.Object, tenantIdProviderMock.Object, _sessionRevocationMock.Object);

        var result = await handler.Handle(new ResetPasswordCommand(_user.Id, "token", "New#1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // Anonymous request - no ambient tenant, so the password write must run under the user's own.
        tenantIdProviderMock.Verify(p => p.Impersonate(_user.TenantId), Times.Once);
        VerifyRevoked(SessionRevocationReasons.PasswordChanged);
    }

    [Fact]
    public async Task AdminResetPassword_Success_RevokesTheTargetUsersSessions()
    {
        var admin = new AppUser { Id = Guid.NewGuid(), TenantId = _user.TenantId, UserName = "admin", Email = "admin@test.com" };
        _userContextMock.Setup(c => c.UserId).Returns(admin.Id);
        _userRepoMock.Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _userRepoMock.Setup(r => r.CheckPasswordAsync(admin, "Admin#1234")).ReturnsAsync(true);
        _userRepoMock.Setup(r => r.AdminResetPasswordAsync(_user, "New#1234")).ReturnsAsync(IdentityResult.Success);
        var handler = new AdminResetUserPasswordCommandHandler(
            _userRepoMock.Object, _userContextMock.Object, _publisherMock.Object, _sessionRevocationMock.Object);

        var result = await handler.Handle(
            new AdminResetUserPasswordCommand(_user.Id, "Admin#1234", "New#1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        VerifyRevoked(SessionRevocationReasons.PasswordReset);
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    private AssignRolesToUserCommandHandler RolesHandler(params string[] currentRoles)
    {
        _userRepoMock.Setup(r => r.GetUserRoleAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(currentRoles);
        _userRepoMock.Setup(r => r.ReplaceRolesAsync(_user, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityResult.Success, new List<string>()));
        return new AssignRolesToUserCommandHandler(
            _userRepoMock.Object, Mock.Of<IPermissionCacheInvalidator>(), _userContextMock.Object,
            _publisherMock.Object, _sessionRevocationMock.Object);
    }

    [Fact]
    public async Task AssignRoles_ChangedSet_RevokesAllSessions()
    {
        var handler = RolesHandler("Employee");

        await handler.Handle(new AssignRolesToUserCommand(_user.Id, ["Admin"]), CancellationToken.None);

        VerifyRevoked(SessionRevocationReasons.RolesChanged);
    }

    [Fact]
    public async Task AssignRoles_SameSet_RevokesNothing()
    {
        var handler = RolesHandler("Employee", "Accountant");

        await handler.Handle(new AssignRolesToUserCommand(_user.Id, ["accountant", "employee"]), CancellationToken.None);

        VerifyNotRevoked();
    }

    // ── Permission overrides ─────────────────────────────────────────────────

    private static readonly string SomePermission = Permissions.All.First(p => !p.StartsWith("platform."));

    private UpdateUserPermissionsCommandHandler PermissionsHandler(params UserPermissionOverride[] existing)
    {
        var userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock.Setup(m => m.FindByIdAsync(_user.Id.ToString())).ReturnsAsync(_user);
        var overrideRepoMock = new Mock<IUserPermissionOverrideRepository>();
        overrideRepoMock.Setup(r => r.GetForUserAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing.ToList());
        return new UpdateUserPermissionsCommandHandler(
            userManagerMock.Object, overrideRepoMock.Object, Mock.Of<IPermissionCacheInvalidator>(),
            _sessionRevocationMock.Object);
    }

    [Fact]
    public async Task UpdatePermissions_ChangedOverrides_RevokesAllSessions()
    {
        var handler = PermissionsHandler(new UserPermissionOverride { Permission = SomePermission, Effect = PermissionEffect.Allow });

        await handler.Handle(new UpdateUserPermissionsCommand(
            _user.Id, [new PermissionOverrideInput(SomePermission, PermissionEffect.Deny)]), CancellationToken.None);

        VerifyRevoked(SessionRevocationReasons.PermissionsChanged);
    }

    [Fact]
    public async Task UpdatePermissions_SameOverrides_RevokesNothing()
    {
        var handler = PermissionsHandler(new UserPermissionOverride { Permission = SomePermission, Effect = PermissionEffect.Allow });

        await handler.Handle(new UpdateUserPermissionsCommand(
            _user.Id, [new PermissionOverrideInput(SomePermission, PermissionEffect.Allow)]), CancellationToken.None);

        VerifyNotRevoked();
    }

    // ── Branch access ────────────────────────────────────────────────────────

    private UpdateUserBranchesCommandHandler BranchesHandler(params int[] existingBranchIds)
    {
        _userRepoMock.Setup(r => r.GetUserRoleAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(["Employee"]);
        var accessRepoMock = new Mock<IUserBranchAccessRepository>();
        accessRepoMock.Setup(r => r.GetForUserAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBranchIds.Select(id => new UserBranchAccess { UserId = _user.Id, BranchId = id }).ToList());
        return new UpdateUserBranchesCommandHandler(
            _userRepoMock.Object, accessRepoMock.Object, Mock.Of<IBranchRepository>(), _userContextMock.Object,
            _publisherMock.Object, _sessionRevocationMock.Object);
    }

    [Fact]
    public async Task UpdateBranches_ChangedSet_RevokesAllSessions()
    {
        // Removing a branch (rather than adding one) keeps NewlyAddedBranchGuard out of the way.
        var handler = BranchesHandler(1, 2);

        await handler.Handle(new UpdateUserBranchesCommand(_user.Id, [1]), CancellationToken.None);

        VerifyRevoked(SessionRevocationReasons.PermissionsChanged);
    }

    [Fact]
    public async Task UpdateBranches_SameSet_RevokesNothing()
    {
        var handler = BranchesHandler(1, 2);

        await handler.Handle(new UpdateUserBranchesCommand(_user.Id, [2, 1]), CancellationToken.None);

        VerifyNotRevoked();
    }
}
