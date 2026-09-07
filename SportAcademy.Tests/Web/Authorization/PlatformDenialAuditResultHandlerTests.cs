using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Authorization;

namespace SportAcademy.Tests.Web.Authorization;

// Unit tests for the decorator that records a Denied TenantAuditEvent when an authenticated
// user is refused a /api/platform route (F-08). No WebApplicationFactory/full host involved -
// the handler is exercised directly against a DefaultHttpContext whose RequestServices carries
// just the services WriteDeniedEventAsync and the real AuthorizationMiddlewareResultHandler it
// decorates actually need (IUserContextService, ITenantAuditRepository, IAuthenticationService
// for the Forbid/Challenge call the decorated default handler makes).
public class PlatformDenialAuditResultHandlerTests
{
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<ITenantAuditRepository> _auditRepositoryMock = new();
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly PlatformDenialAuditResultHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public PlatformDenialAuditResultHandlerTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_userId);
        _userContextMock.Setup(c => c.IpAddress).Returns("198.51.100.7");
        _userContextMock.Setup(c => c.UserAgent).Returns("test-agent");

        _authServiceMock
            .Setup(a => a.ForbidAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);
        _authServiceMock
            .Setup(a => a.ChallengeAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        _handler = new PlatformDenialAuditResultHandler(Mock.Of<ILogger<PlatformDenialAuditResultHandler>>());
    }

    private HttpContext BuildContext(string path, bool authenticated)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_userContextMock.Object);
        services.AddSingleton(_auditRepositoryMock.Object);
        services.AddSingleton(_authServiceMock.Object);

        // A ClaimsIdentity's IsAuthenticated is true only when constructed with a non-null
        // authenticationType - the same thing a real cookie/JWT auth handler does once a token
        // validates. No authenticationType (the parameterless ctor) mirrors an anonymous caller.
        var identity = authenticated ? new ClaimsIdentity(authenticationType: "Test") : new ClaimsIdentity();

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            User = new ClaimsPrincipal(identity),
        };
        context.Request.Path = path;
        return context;
    }

    private static AuthorizationPolicy RolePolicy(params string[] roles) =>
        new AuthorizationPolicyBuilder().RequireRole(roles).Build();

    [Fact]
    public async Task HandleAsync_DeniedOnPlatformRoute_WritesDeniedEventWithRoleReason()
    {
        TenantAuditEvent? captured = null;
        _auditRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAuditEvent, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        var context = BuildContext("/api/platform/tenants", authenticated: true);
        context.Request.Method = "DELETE";
        var policy = RolePolicy("SuperAdmin");

        await _handler.HandleAsync(_ => Task.CompletedTask, context, policy, PolicyAuthorizationResult.Forbid());

        captured.Should().NotBeNull();
        captured!.TenantId.Should().BeNull();
        captured.EventType.Should().Be("platform.access_denied");
        captured.Outcome.Should().Be(AuditOutcome.Denied);
        captured.Reason.Should().Contain("SuperAdmin");
        captured.PerformedByUserId.Should().Be(_userId);
        captured.IpAddress.Should().Be("198.51.100.7");
        captured.Description.Should().Contain("DELETE").And.Contain("/api/platform/tenants");

        // The decorator must still delegate to the real default handler so the actual 403
        // response gets written - auditing is additive, never a replacement.
        _authServiceMock.Verify(
            a => a.ForbidAsync(context, null, null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NotAuthenticated_WritesNoAuditEventButStillChallenges()
    {
        var context = BuildContext("/api/platform/tenants", authenticated: false);
        var policy = RolePolicy("SuperAdmin");

        await _handler.HandleAsync(_ => Task.CompletedTask, context, policy, PolicyAuthorizationResult.Challenge());

        _auditRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _authServiceMock.Verify(a => a.ChallengeAsync(context, null, null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonPlatformRoute_WritesNoAuditEvent()
    {
        var context = BuildContext("/api/trainees", authenticated: true);
        var policy = RolePolicy("Owner");

        await _handler.HandleAsync(_ => Task.CompletedTask, context, policy, PolicyAuthorizationResult.Forbid());

        _auditRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AuthorizationSucceeded_WritesNoAuditEventAndCallsNext()
    {
        var context = BuildContext("/api/platform/tenants", authenticated: true);
        var policy = RolePolicy("SuperAdmin");
        var nextCalled = false;

        await _handler.HandleAsync(
            _ => { nextCalled = true; return Task.CompletedTask; },
            context, policy, PolicyAuthorizationResult.Success());

        nextCalled.Should().BeTrue();
        _auditRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AuditWriteThrows_StillDelegatesToDefaultHandlerInstead()
    {
        // A bug in the audit write itself must never turn into a 500 on every denied platform
        // request - it's caught and logged, and the real 403/challenge still goes out.
        _auditRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TenantAuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var context = BuildContext("/api/platform/tenants", authenticated: true);
        var policy = RolePolicy("SuperAdmin");

        Func<Task> act = () => _handler.HandleAsync(_ => Task.CompletedTask, context, policy, PolicyAuthorizationResult.Forbid());

        await act.Should().NotThrowAsync();
        _authServiceMock.Verify(a => a.ForbidAsync(context, null, null), Times.Once);
    }
}
