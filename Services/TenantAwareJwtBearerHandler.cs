using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public class TenantAwareJwtBearerHandler : JwtBearerHandler
    {
        private readonly JwtSettings _jwtSettings;

        public TenantAwareJwtBearerHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock timeProvider, JwtSettings jwtSettings)
            : base(options, logger, encoder, timeProvider)
        {
            _jwtSettings = jwtSettings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get tenant name from URL route
            var tenantName = Context.Request.RouteValues["tenantName"]?.ToString();
            
            if (string.IsNullOrEmpty(tenantName))
            {
                Logger.LogWarning("No tenant name found in route");
                return Task.FromResult(AuthenticateResult.Fail("Tenant name required in URL"));
            }

            // Get the authorization header
            string authorization = Request.Headers.Authorization.ToString();
            
            if (string.IsNullOrEmpty(authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Authorization header"));
            }

            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("No token provided"));
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                // Validate token with tenant-specific issuer and audience
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tenantName, // Must match tenant from URL
                    ValidAudience = tenantName, // Must match tenant from URL
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                // Additional validation: ensure tenant claim matches URL tenant
                var tokenTenant = principal.FindFirst("tenant")?.Value;
                if (!tenantName.Equals(tokenTenant, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarning("Token tenant '{TokenTenant}' does not match URL tenant '{UrlTenant}'", tokenTenant, tenantName);
                    return Task.FromResult(AuthenticateResult.Fail("Token tenant mismatch"));
                }

                Logger.LogInformation("Successfully validated JWT for tenant: {TenantName}", tenantName);

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (SecurityTokenExpiredException)
            {
                Logger.LogWarning("Token expired for tenant: {TenantName}", tenantName);
                return Task.FromResult(AuthenticateResult.Fail("Token expired"));
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                Logger.LogWarning("Invalid token signature for tenant: {TenantName}", tenantName);
                return Task.FromResult(AuthenticateResult.Fail("Invalid token signature"));
            }
            catch (SecurityTokenValidationException ex)
            {
                Logger.LogWarning("Token validation failed for tenant {TenantName}: {Error}", tenantName, ex.Message);
                return Task.FromResult(AuthenticateResult.Fail($"Token validation failed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during token validation for tenant: {TenantName}", tenantName);
                return Task.FromResult(AuthenticateResult.Fail("Token validation error"));
            }
        }
    }
}