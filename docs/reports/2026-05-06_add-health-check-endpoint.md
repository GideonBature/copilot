# Change Report — Add HealthCheck Endpoint

**Date:** 2026-05-06
**Author:** Copilot / AI Agent
**Version:** 1.0.0

---

## Summary

Added a `GET /v1/health` endpoint to the FirstBank Copilot API. The endpoint returns
HTTP 200 OK with a JSON body containing `status = "Healthy"` and `service = "Copilot API"`.
The endpoint is explicitly marked `[AllowAnonymous]` as required by the FirstBank security
standards for HealthCheck endpoints.

---

## Features Implemented

- **HealthCheck Endpoint**: `GET /v1/health` — returns 200 OK with health status and service name.
  This endpoint is unauthenticated so that infrastructure monitoring tools can poll it without credentials.

---

## Files Modified

### Domain Layer
- `FirstBankNigeria.Copilot.Domain/AssemblyMarker.cs` — Created (assembly reference marker for architecture tests)

### Application Layer
- `FirstBankNigeria.Copilot.Application/DependencyInjection.cs` — Created
- `FirstBankNigeria.Copilot.Application/Features/Health/Queries/GetHealth/GetHealthQuery.cs` — Created
- `FirstBankNigeria.Copilot.Application/Features/Health/Queries/GetHealth/GetHealthQueryHandler.cs` — Created
- `FirstBankNigeria.Copilot.Application/Features/Health/Queries/GetHealth/GetHealthResponse.cs` — Created

### Infrastructure Layer
- `FirstBankNigeria.Copilot.Infrastructure/DependencyInjection.cs` — Created (stub; no infrastructure services required for HealthCheck)

### API Layer
- `FirstBankNigeria.Copilot.Api/Program.cs` — Created
- `FirstBankNigeria.Copilot.Api/appsettings.json` — Created
- `FirstBankNigeria.Copilot.Api/appsettings.Development.json` — Created
- `FirstBankNigeria.Copilot.Api/Properties/launchSettings.json` — Created
- `FirstBankNigeria.Copilot.Api/Controllers/BaseController.cs` — Created (`[Authorize]` enforced globally)
- `FirstBankNigeria.Copilot.Api/Controllers/v1/HealthController.cs` — Created (`[AllowAnonymous]`, `GET /v1/health`)
- `FirstBankNigeria.Copilot.Api/Middleware/GlobalExceptionHandlerMiddleware.cs` — Created

---

## Database Changes

None — the HealthCheck endpoint does not access the database.

---

## NuGet Packages

| Package | Action | Justification |
|---------|--------|---------------|
| `MediatR 12.4.1` | Added | CQRS query dispatching in Application layer |
| `FluentValidation 11.11.0` | Added | Mandatory Application-layer package |
| `FluentValidation.DependencyInjectionExtensions 11.11.0` | Added | Mandatory Application-layer package |
| `Microsoft.Extensions.Caching.Abstractions 8.0.0` | Added | Mandatory Application-layer package |
| `Microsoft.Extensions.Logging.Abstractions 8.0.0` | Added | Mandatory Application-layer package |
| `Dapper 2.1.35` | Added | Mandatory Infrastructure-layer package |
| `Oracle.ManagedDataAccess.Core 23.7.0` | Added | Mandatory Infrastructure-layer package |
| `Serilog.AspNetCore 8.0.3` | Added | Structured logging (API + Infrastructure) |
| `Serilog.Enrichers.Thread 4.0.0` | Added | Thread ID enrichment |
| `Serilog.Enrichers.Environment 3.0.0` | Added | Machine name enrichment |
| `Microsoft.Extensions.Caching.Memory 8.0.1` | Added | Mandatory Infrastructure-layer package |
| `Swashbuckle.AspNetCore 6.9.0` | Added | Swagger UI for API layer |
| `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.15` | Added | JWT Bearer authentication |

---

## Configuration Changes

- `appsettings.json` — Added `Jwt:Authority`, `Jwt:Audience` keys (values populated at deployment); Serilog configuration with console sink
- `appsettings.Development.json` — Added Debug-level logging override
- `Properties/launchSettings.json` — Added `http` and `https` profiles; both set `launchUrl = "swagger"`

---

## API Endpoint Changes

| Method | Route | Action | Description |
|--------|-------|--------|-------------|
| GET | `/v1/health` | New | Returns `{ status: "Healthy", service: "Copilot API" }` — unauthenticated |

---

## Breaking Changes

None — this is a new solution. No existing consumers are affected.

---

## Testing Summary

- Unit tests added for `GetHealthQueryHandler` — 3 test cases (all passing):
  - `Handle_ValidQuery_ReturnsHealthyStatus`
  - `Handle_ValidQuery_ReturnsCorrectServiceName`
  - `Handle_ValidQuery_ReturnsNonNullResponse`
- Integration tests added for `GET /v1/health` — 4 test cases (all passing):
  - `Get_HealthEndpoint_Returns200Ok`
  - `Get_HealthEndpoint_ReturnsHealthyStatus`
  - `Get_HealthEndpoint_ReturnsCorrectServiceName`
  - `Get_HealthEndpoint_DoesNotRequireAuthentication`
- Architecture tests verified — 4 layer dependency rules enforced (all passing):
  - API must not depend on Infrastructure ✅
  - Application must not depend on Infrastructure ✅
  - Domain must not depend on MediatR/Dapper/FluentValidation ✅
  - Domain must not depend on Application ✅

---

## Known Issues / Limitations

- `Jwt:Authority` and `Jwt:Audience` are left blank in `appsettings.json` — these must be populated with the actual identity provider values at deployment time.
- The Infrastructure project is a stub; repository implementations will be added when database-backed features are introduced.

---

## Build Verification

- **Build Status:** Pass ✅
- **Test Results:** 11 / 11 passing ✅
- **Architecture Tests:** All 4 passing ✅
- **Warnings:** None
