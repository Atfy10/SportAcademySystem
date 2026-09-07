using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Middleware;

namespace SportAcademy.Tests.Web.Middleware;

// Unit tests for the enforcement point that rejects a request from a non-Active tenant (F-02).
// Exercised directly against a DefaultHttpContext with an in-memory response stream, rather than
// through a full host - InvokeAsync takes its dependencies as method parameters (not
// constructor-injected), so this only needs mocks for IUserContextService/ITenantStatusCache.
public class TenantStatusGuardMiddlewareTests
{
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<ITenantStatusCache> _statusCacheMock = new();
    private bool _nextCalled;

    private static HttpContext BuildContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private TenantStatusGuardMiddleware BuildMiddleware()
    {
        _nextCalled = false;
        RequestDelegate next = _ => { _nextCalled = true; return Task.CompletedTask; };
        return new TenantStatusGuardMiddleware(next);
    }

    private static async Task<JsonElement> ReadJsonBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedRequest_PassesThroughWithoutCheckingStatus()
    {
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(false);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
        _statusCacheMock.Verify(
            c => c.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedWithNoTenantClaim_PassesThroughWithoutCheckingStatus()
    {
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns((Guid?)null);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
        _statusCacheMock.Verify(
            c => c.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/platform/tenants")]
    [InlineData("/health")]
    public async Task InvokeAsync_ExemptPath_PassesThroughEvenForANonActiveTenant(string path)
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Suspended);
        var context = BuildContext(path);
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
        _statusCacheMock.Verify(
            c => c.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ActiveTenant_PassesThrough()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Active);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200); // DefaultHttpContext's untouched default
    }

    [Fact]
    public async Task InvokeAsync_SuspendedTenant_Returns403WithSuspendedCodeAndStopsThePipeline()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Suspended);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("TENANT_SUSPENDED");
    }

    [Fact]
    public async Task InvokeAsync_ArchivedTenant_Returns403WithArchivedCode()
    {
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Archived);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("TENANT_ARCHIVED");
    }

    [Fact]
    public async Task InvokeAsync_TenantVanishedFromCache_TreatedAsSuspendedNotLetThrough()
    {
        // A valid token always carries a real tenant id, so this should be unreachable - but the
        // middleware treats a null lookup as locked-out rather than defaulting to "let it pass".
        var tenantId = Guid.NewGuid();
        _userContextMock.Setup(c => c.IsAuthenticated).Returns(true);
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _statusCacheMock
            .Setup(c => c.GetStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantStatus?)null);
        var context = BuildContext("/api/trainees");
        var middleware = BuildMiddleware();

        await middleware.InvokeAsync(context, _userContextMock.Object, _statusCacheMock.Object);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var body = await ReadJsonBodyAsync(context);
        body.GetProperty("errors").GetProperty("code").GetString().Should().Be("TENANT_SUSPENDED");
    }
}
