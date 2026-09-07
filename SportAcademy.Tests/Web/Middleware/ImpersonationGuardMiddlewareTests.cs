using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Web.Middleware;

namespace SportAcademy.Tests.Web.Middleware;

// Unit tests for the two guarantees an impersonation session promises (Phase 3): read-only, and
// revocable before its own JWT expiry. Exercised directly against a DefaultHttpContext - the
// middleware's dependencies (IUserContextService, IImpersonationGrantRepository) are method
// parameters, not constructor-injected, so no host is needed.
public class ImpersonationGuardMiddlewareTests
{
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IImpersonationGrantRepository> _grantRepositoryMock = new();
    private bool _nextCalled;

    private static HttpContext BuildContext(string path, string method)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private ImpersonationGuardMiddleware BuildMiddleware()
    {
        _nextCalled = false;
        RequestDelegate next = _ => { _nextCalled = true; return Task.CompletedTask; };
        return new ImpersonationGuardMiddleware(next, Mock.Of<ILogger<ImpersonationGuardMiddleware>>());
    }

    private static TenantImpersonationGrant ActiveGrant(Guid id, Guid tenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        GrantedByUserId = Guid.NewGuid(),
        Reason = "test",
        StartedAt = DateTime.UtcNow.AddMinutes(-5),
        ExpiresAt = DateTime.UtcNow.AddMinutes(55),
    };

    private static async Task<JsonElement> ReadJsonBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    [Fact]
    public async Task InvokeAsync_NoActiveGrant_PassesThroughWithoutTouchingTheRepository()
    {
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns((Guid?)null);
        var context = BuildContext("/api/trainees", "POST");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeTrue();
        _grantRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("/api/platform/impersonation/abc/end", "POST")]
    [InlineData("/api/auth/refresh", "POST")]
    public async Task InvokeAsync_ExemptPath_PassesThroughEvenForAWrite(string path, string method)
    {
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns(Guid.NewGuid());
        var context = BuildContext(path, method);
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeTrue();
        _grantRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task InvokeAsync_WriteMethodOnNonExemptPath_Returns403ReadOnlyWithoutCheckingTheGrant(string method)
    {
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns(Guid.NewGuid());
        var context = BuildContext("/api/trainees", method);
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        // The read-only rejection is a pure method check - it must not need a DB round trip to
        // reject a write, unlike the expiry check below which does.
        _grantRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("IMPERSONATION_READ_ONLY");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_ReadMethodWithActiveGrant_PassesThrough(string method)
    {
        var grantId = Guid.NewGuid();
        var grant = ActiveGrant(grantId, Guid.NewGuid());
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns(grantId);
        _grantRepositoryMock
            .Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);
        var context = BuildContext("/api/trainees", method);
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_GrantNotFound_Returns401Ended()
    {
        var grantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns(grantId);
        _grantRepositoryMock
            .Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantImpersonationGrant?)null);
        var context = BuildContext("/api/trainees", "GET");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("IMPERSONATION_ENDED");
    }

    [Fact]
    public async Task InvokeAsync_GrantAlreadyEnded_Returns401EndedEvenOnAGet()
    {
        var grantId = Guid.NewGuid();
        var grant = ActiveGrant(grantId, Guid.NewGuid());
        grant.EndedAt = DateTime.UtcNow.AddMinutes(-1);
        _userContextMock.Setup(c => c.ImpersonationGrantId).Returns(grantId);
        _grantRepositoryMock
            .Setup(r => r.GetByIdAsync(grantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);
        var context = BuildContext("/api/trainees", "GET");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _grantRepositoryMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("IMPERSONATION_ENDED");
    }
}
