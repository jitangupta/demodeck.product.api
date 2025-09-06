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
            try
            {
                // Extract tenant information from JWT claims
                var tenantId = context.User.FindFirst("tenant_id")?.Value;
                var tenantName = context.User.FindFirst("tenant")?.Value;
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = context.User.FindFirst("user_role")?.Value;

                // Validate tenant name from URL matches JWT token
                var routeTenantName = context.Request.RouteValues["tenantName"]?.ToString();
                
                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(tenantName))
                {
                    _logger.LogWarning("No tenant information found in JWT token");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Tenant information required");
                    return;
                }

                if (!string.IsNullOrEmpty(routeTenantName) && 
                    !tenantName.Equals(routeTenantName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Tenant mismatch: JWT={TenantName}, Route={RouteTenant}", 
                        tenantName, routeTenantName);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Tenant access denied");
                    return;
                }

                // Set tenant context
                var tenantContext = new TenantContext
                {
                    TenantId = tenantId,
                    TenantName = tenantName,
                    UserId = userId ?? string.Empty,
                    UserRole = userRole ?? "User"
                };

                tenantContextService.SetContext(tenantContext);

                _logger.LogInformation("Tenant context set: {TenantId} for user {UserId}", tenantId, userId);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tenant context");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }
    }
}