using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Demodeck.Product.Api.Middleware;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Tests.Middleware;

public class TenantContextMiddlewareTests
{
    private readonly Mock<ITenantContextService> _mockTenantContext;
    private readonly Mock<ILogger<TenantContextMiddleware>> _mockLogger;

    public TenantContextMiddlewareTests()
    {
        _mockTenantContext = new Mock<ITenantContextService>();
        _mockLogger = new Mock<ILogger<TenantContextMiddleware>>();
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedWithAllClaims_SetsContextAndCallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantContextMiddleware(next, _mockLogger.Object);

        var context = new DefaultHttpContext();
        context.User = CreateAuthenticatedUser(
            new Claim("tenant_id", "tnt_acme001"),
            new Claim("tenant", "acme"),
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim("user_role", "Admin"));

        await middleware.InvokeAsync(context, _mockTenantContext.Object);

        nextCalled.Should().BeTrue();
        _mockTenantContext.Verify(x => x.SetContext(It.Is<TenantContext>(tc =>
            tc.TenantId == "tnt_acme001" &&
            tc.TenantName == "acme" &&
            tc.UserId == "user123" &&
            tc.UserRole == "Admin")), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedMissingClaims_SetsDefaultValues()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantContextMiddleware(next, _mockLogger.Object);

        var context = new DefaultHttpContext();
        // Authenticated but with no relevant claims
        context.User = CreateAuthenticatedUser();

        await middleware.InvokeAsync(context, _mockTenantContext.Object);

        nextCalled.Should().BeTrue();
        _mockTenantContext.Verify(x => x.SetContext(It.Is<TenantContext>(tc =>
            tc.TenantId == string.Empty &&
            tc.TenantName == string.Empty &&
            tc.UserId == string.Empty &&
            tc.UserRole == "User")), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_NotAuthenticated_SkipsSetContextAndCallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantContextMiddleware(next, _mockLogger.Object);

        var context = new DefaultHttpContext(); // User.Identity.IsAuthenticated is false by default

        await middleware.InvokeAsync(context, _mockTenantContext.Object);

        nextCalled.Should().BeTrue();
        _mockTenantContext.Verify(x => x.SetContext(It.IsAny<TenantContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_SetContextThrows_Returns500()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantContextMiddleware(next, _mockLogger.Object);

        _mockTenantContext.Setup(x => x.SetContext(It.IsAny<TenantContext>()))
            .Throws(new Exception("Context error"));

        var context = new DefaultHttpContext();
        context.User = CreateAuthenticatedUser(new Claim("tenant_id", "tnt_acme001"));

        await middleware.InvokeAsync(context, _mockTenantContext.Object);

        context.Response.StatusCode.Should().Be(500);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_Authenticated_LogsInformation()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new TenantContextMiddleware(next, _mockLogger.Object);

        var context = new DefaultHttpContext();
        context.User = CreateAuthenticatedUser(
            new Claim("tenant_id", "tnt_acme001"),
            new Claim(ClaimTypes.NameIdentifier, "user123"));

        await middleware.InvokeAsync(context, _mockTenantContext.Object);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
