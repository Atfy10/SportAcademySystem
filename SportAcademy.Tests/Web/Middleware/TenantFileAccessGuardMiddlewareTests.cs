using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Middleware;

namespace SportAcademy.Tests.Web.Middleware;

// No existing coverage before this - added alongside R2 (PLAN_LIMITS_DESIGN.md), the audit that
// found this middleware needed the same PendingLimitSelection widening as
// TenantStatusGuardMiddleware but has no verb distinction to make (a file request is always a
// read).
public class TenantFileAccessGuardMiddlewareTests
{
    private readonly Mock<ITenantStatusCache> _statusCacheMock = new();
    private bool _nextCalled;

    private static HttpContext BuildContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }

    private TenantFileAccessGuardMiddleware BuildMiddleware()
    {
        _nextCalled = false;
        RequestDelegate next = _ => { _nextCalled = true; return Task.CompletedTask; };
        return new TenantFileAccessGuardMiddleware(next);
    }

    [Fact]
    public async Task InvokeAsync_NotAnUploadsPath_PassesThroughWithoutCheckingStatus()
    {
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
        _statusCacheMock.Verify(c => c.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ActiveTenant_PassesThrough()
    {
        var tenantId = Guid.NewGuid();
        _statusCacheMock.Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(TenantStatus.Active);
        var context = BuildContext($"/uploads/{tenantId:N}/avatars/1.png");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PendingLimitSelectionTenant_PassesThrough()
    {
        // The whole point of this fix: the read-only console must not render with every image
        // broken during the 7-day forced-selection window.
        var tenantId = Guid.NewGuid();
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.PendingLimitSelection);
        var context = BuildContext($"/uploads/{tenantId:N}/avatars/1.png");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SuspendedTenant_Returns403()
    {
        var tenantId = Guid.NewGuid();
        _statusCacheMock.Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(TenantStatus.Suspended);
        var context = BuildContext($"/uploads/{tenantId:N}/avatars/1.png");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _statusCacheMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
}
