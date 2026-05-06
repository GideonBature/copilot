---
name: firstbank-csharp
description: >
  Core C# coding standards and implementation rules for FirstBank Nigeria. Use this skill
  whenever writing, reviewing, or modifying any C# code for a FirstBank solution — including
  naming conventions, security requirements, data access patterns, logging, error handling,
  NuGet packages, and the mandatory change report template. Also triggers for questions about
  FirstBank's Oracle database conventions, API design rules, branching strategy, or any
  csharp.instructions.md reference. Always consult this skill before writing a single line of
  C# code in any FirstBank project.
---

# FirstBank C# Standards — Core Reference

> This skill is the **authoritative source** for all C# implementation decisions at FirstBank Nigeria.
> All other FirstBank skills (`firstbank-new-project`, `firstbank-existing-project`) depend on this one.
> When in doubt, re-read the relevant section here before proceeding.

---

## Quick Reference — Most-Violated Rules

| Rule | Standard |
|------|----------|
| Database access | Oracle stored procedures **only** — no inline SQL |
| ORM | Dapper with `CommandType.StoredProcedure` |
| Hashing | SHA-256 only — never SHA-512 |
| Endpoints | HTTPS only — never HTTP |
| Auth | JWT Bearer; `[Authorize]` globally; `[AllowAnonymous]` for Login/HealthCheck only |
| Method length | 25–30 lines max |
| Logging | Serilog (preferred) or log4net — never `Console.WriteLine` |
| File creation | Manual only — no CLI generators |
| Commented-out code | **Delete it** — not permitted in any deployment-ready file |
| Change report | Mandatory — task is **not** complete without one |

---

## 1. General C# Rules (Section 2.1)

- **Target**: .NET 8.0 / ASP.NET Core Web API
- **Structure**: Clean Architecture — no layer leakage
- **File/directory creation**: Manual only — do NOT use CLI scaffolding tools
- **Commented-out code**: Must not appear in any deployment-ready file. Delete it entirely.
- **Dependencies**: NuGet only — no manual binary drops

---

## 2. Security (Section 2.2)

- All endpoints **HTTPS only** — no HTTP exposure ever
- **SHA-256** for all hashing — never SHA-512
- JWT Bearer authentication mandatory for all endpoints
- `[Authorize]` at `BaseController` level; `[AllowAnonymous]` only for Login/HealthCheck
- Claims-based authorization (`sub`, `roles`) — not role booleans
- Disable concurrent logins on all web applications
- Maker/Checker controls for every create/update/delete operation
- **Audit trail** must capture: User ID, Timestamp, IP Address, Activity Type, Activity Details, Comments
- Oracle: dedicated schema user — **never** SYS or SYSTEM
- Connection strings **encrypted** in `appsettings.json` — no plain-text secrets
- Usernames stored and compared in **lowercase**
- Rate limiting: return `503` + `Retry-After` header when limits exceeded
- Data masking with asterisks for sensitive data in logs/UI

---

## 3. Data Access (Section 2.4 & 2.5)

### The Non-Negotiables
- Standard database: **Oracle Database**
- Client library: `Oracle.ManagedDataAccess.Core` — not `System.Data.OracleClient`
- **All DB access through stored procedures** — inline SQL is forbidden
- Stored procedures called via **Dapper** with `CommandType.StoredProcedure`
- All parameters via `OracleParameter` — never string concatenation

### Naming Conventions
| Object | Convention | Example |
|--------|-----------|---------|
| Stored procedure | `SP_<ACTION>_<ENTITY>` | `SP_GET_CUSTOMER`, `SP_CREATE_ORDER` |
| Package | `PKG_<ENTITY>` | `PKG_CUSTOMER`, `PKG_AUDIT` |
| Table | `<PREFIX>_<NAME>_IN_UPPERCASE` | `EV_PARTICIPANTS_LIST` |
| Sequence | `SEQ_<TABLE_NAME>` | `SEQ_CUSTOMER` |

### Other Rules
- Use `SYS_REFCURSOR` for all result-set outputs
- Explicit JOINs only — no implicit comma joins
- No `LIKE` on unindexed columns without Team Lead approval
- All scripts in `/data/scripts/` — idempotent, version-controlled
- Natural unique identifiers → no surrogate ID column needed

---

## 4. Code Conventions (Section 2.7)

### Naming
| Identifier | Case | Example |
|-----------|------|---------|
| Class | PascalCase | `CustomerRepository` |
| Interface | PascalCase + `I` prefix | `ICustomerRepository` |
| Method | PascalCase | `GetCustomerById` |
| Property | PascalCase | `AccountNumber` |
| Parameter | camelCase | `customerId` |
| Public constant | UPPERCASE | `MAX_RETRY_COUNT` |
| Namespace | `FirstBankNigeria.<SolutionName>.<Layer>.*` | |

### Code Quality
- Methods: **25–30 lines max** — refactor if longer
- No magic numbers — use a constants file
- No commented-out code in any deployable file
- Fix all compiler warnings (exceptions: 612, 618, 619, 1030, 1701, 1702)
- No underscore in identifiers (except constants)
- No abbreviations unless universally understood

---

## 5. Logging (Section 2.10)

- **Framework**: Serilog (preferred) or log4net — never `Console.WriteLine`
- Always include **Thread ID** and **User ID** in every log entry
- Log file naming: `<AppName>_yyyy-MM-dd.log` (one file per day)
- Configured in `Infrastructure/Logging/`; enriched with Thread + User ID

### Log Levels
| Level | When to use |
|-------|------------|
| Fatal | System crash, unrecoverable data integrity violation |
| Error | Operation failed, urgent human intervention needed |
| Warning | Unexpected but recoverable; likely error soon |
| Info | Normal but significant: start/stop, login/logout, boundary events |
| Debug | Normal flow: page rendered, order taken, method entry/exit |

---

## 6. NuGet Packages (Section 2.12.1)

| Layer | Mandatory Packages |
|-------|--------------------|
| Domain | *(none — keep dependency-free)* |
| Application | `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Logging.Abstractions` |
| Infrastructure | `Dapper`, `Oracle.ManagedDataAccess.Core`, `Serilog.AspNetCore`, `Serilog.Enrichers.Thread`, `Serilog.Enrichers.Environment`, `Microsoft.Extensions.Caching.Memory` |
| API | `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Serilog.AspNetCore`, `Serilog.Enrichers.Thread`, `Serilog.Enrichers.Environment` |

Do not add packages outside this list without written justification.

---

## 7. API Design (Section 2.6)

- URI versioning: `/v1/<resource>`
- Resource URIs are **nouns** (plural): `/customers`, `/accounts`
- Use sub-resources for relations: `/customers/123/accounts`
- Paging: `limit` + `offset` query params; default `limit=20, offset=0`; `X-Total-Count` header for total
- Filtering: unique query param per field
- Sorting: ascending/descending over multiple fields

### HTTP Status Codes
| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 204 | No Content (delete) |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 422 | Unprocessable Entity |
| 503 | Rate limit exceeded (include `Retry-After`) |

---

## 8. Error Handling (Section 2.8)

- No empty catch blocks — wrap and rethrow as custom exceptions
- Never catch `NullReferenceException` — test for null before access
- No super-catch (`catch (Exception)`) — catch specific exceptions
- Always `Dispose()` or use `using` for unmanaged resources
- Custom error pages for UAT/Production — never expose stack traces

---

## 9. Source Control (Section 3.2)

| Branch | Purpose | Naming |
|--------|---------|--------|
| `master` | Deployed, tagged, stable | — |
| `develop` | Active development | — |
| `feature/*` | New features | `feature/short-description` |
| `bugfix/*` | Corrections | `bugfix/short-description` |
| `hotfix/*` | Emergency production fix | `hotfix/short-description` |
| `release/*` | Release preparation | `release/x.y.z` |

- Feature branches must be deleted after merge
- Bugfix: branch off `release`, merge back to `release` then `develop`
- Hotfix: branch off `master`, merge to both `master` and `develop`

---

## 10. Mandatory Change Report (Section 2.18)

**No task is complete without a change report.** Save to `/docs/reports/YYYY-MM-DD_<short-description>.md`.

See `references/change-report-template.md` for the full template.

Required sections:
1. Summary
2. Features Implemented
3. Files Modified (by Clean Architecture layer)
4. Database Changes
5. NuGet Packages
6. Configuration Changes
7. API Endpoint Changes
8. Breaking Changes
9. Testing Summary
10. Known Issues / Limitations
11. Build Verification Result

Sections that do not apply → mark as `None`.

---

## Reference Files

- `references/change-report-template.md` — Full mandatory report template
- `references/clean-architecture-structure.md` — Solution folder/project layout
- `references/oracle-script-patterns.md` — Idempotent DDL/DML script patterns
