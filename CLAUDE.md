# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
dotnet build                     # Build the project
dotnet run                       # Run locally (http://localhost:5142)
dotnet publish -c Release        # Publish for production
docker build .                   # Build Docker image
```

## Test Commands

```bash
dotnet test Demodeck.Product.Api.Tests/Demodeck.Product.Api.Tests.csproj                   # Run all tests
dotnet test Demodeck.Product.Api.Tests/Demodeck.Product.Api.Tests.csproj --filter "FullyQualifiedName~HealthControllerTests"  # Run single test class
dotnet test Demodeck.Product.Api.Tests/Demodeck.Product.Api.Tests.csproj --filter "FullyQualifiedName~Get_ReturnsOkResult"    # Run single test
```

Test project uses xUnit + Moq + FluentAssertions. Manual API testing via `test-product.http`.

## Architecture

This is a **multi-tenant product management API** for the DemoDeck SaaS platform, built with ASP.NET Core 8.0 / C# 12. It serves tenant-scoped resources (users, tasks) and relies on JWTs issued by the separate **DemoDeck.Auth.Api** microservice.

### Request Flow

```
GET /{tenantName}/api/user (with Bearer token)
→ TenantAwareJwtBearerHandler (validates JWT, checks tenant claim matches URL tenant)
→ TenantContextMiddleware (extracts claims into AsyncLocal<TenantContext>)
→ Controller (uses ITenantContextService.Current.TenantId to query repository)
→ ApiResponse<T> { Success, Data, Message, ErrorCode }
```

### Key Design Decisions

- **Tenant in URL route:** All resource endpoints use `/{tenantName}/api/[controller]`. The JWT handler validates that the token's `tenant` claim matches the URL's `tenantName` to prevent cross-tenant access.
- **TenantAwareJwtBearerHandler:** Custom `JwtBearerHandler` subclass that validates JWT issuer/audience against the URL tenant name (not global settings). Validates signature with shared `JwtSettings.SecretKey`.
- **AsyncLocal tenant context:** `TenantContextService` uses `AsyncLocal<TenantContext>` for thread-safe per-request tenant isolation. Registered as scoped.
- **In-memory repositories:** `InMemoryUserRepository` and `InMemoryTaskRepository` hold seed data for 4 test tenants (acme, globalx, initech, umbrella). Both are registered as singletons.
- **Middleware ordering:** Authentication → TenantContextMiddleware → Authorization. The middleware only sets context for authenticated requests.

### Endpoints

- `GET /health` — Health check (no auth)
- `GET /{tenantName}/api/user` — List active users for tenant (auth required)
- `GET /{tenantName}/api/task` — List tasks for tenant (auth required)
- `GET /swagger/` — Swagger UI (Development only)

### JWT Claims

Tokens must include: `tenant` (must match URL), `tenant_id`, `user_role`, `ClaimTypes.NameIdentifier`. Issuer and audience are validated against the URL tenant name.

## CI/CD

GitHub Actions workflow (`.github/workflows/docker-image.yml`) is manually triggered via `workflow_dispatch`, accepts a tag input, and pushes a Docker image to `ghcr.io`.

## Gotchas

- **Nested test project:** Main `.csproj` has `<DefaultItemExcludes>` to exclude `Demodeck.Product.Api.Tests\**` since the test directory lives inside the main project directory.
- **TenantAwareJwtBearerHandler uses deprecated ISystemClock:** Produces CS0618 warnings at build time. This is inherited from `JwtBearerHandler` and is cosmetic.
- **JWT issuer/audience = tenant name:** Unlike typical JWT setups, issuer and audience are validated against the URL's `tenantName`, not global config values. The Auth API must issue tokens with tenant-specific issuer/audience.
- **Repository filtering differences:** `InMemoryUserRepository` filters by `TenantId && IsActive`; `InMemoryTaskRepository` filters by `TenantId` only.
