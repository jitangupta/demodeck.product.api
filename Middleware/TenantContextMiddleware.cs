using System.Security.Claims;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Services;

namespace Demodeck.Product.Api.Middleware
{
    public class TenantContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantContextMiddleware> _logger;

        public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContextService tenantContextService)
        {
            // Only process if user is authenticated (JWT validation already done by TenantAwareJwtBearerHandler)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Extract tenant information from validated JWT claims
                    var tenantId = context.User.FindFirst("tenant_id")?.Value;
                    var tenantName = context.User.FindFirst("tenant")?.Value;
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userRole = context.User.FindFirst("user_role")?.Value;

                    // Set tenant context (validation already done in JWT handler)
                    var tenantContext = new TenantContext
                    {
                        TenantId = tenantId ?? string.Empty,
                        TenantName = tenantName ?? string.Empty,
                        UserId = userId ?? string.Empty,
                        UserRole = userRole ?? "User"
                    };

                    tenantContextService.SetContext(tenantContext);

                    _logger.LogInformation("Tenant context set: {TenantId} for user {UserId}", tenantId, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting tenant context");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal server error");
                    return;
                }
            }

            await _next(context);
        }
    }
}