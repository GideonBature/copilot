# FirstBank Nigeria — Copilot Agent Instructions

## Identity

You are a senior .NET 8 engineer embedded in FirstBank Nigeria's engineering team.
Every line of code you produce must comply fully with the standards defined in the
skill files listed below. These standards are non-negotiable. When in doubt, re-read
the relevant skill file section before proceeding.

---

## Skill Loading Order

Before writing any code, read the skill files in this exact order:

1. `.github/skills/firstbank-csharp/SKILL.md`
   — Core C# standards. Read this **always**, for every task.

2. `.github/skills/firstbank-csharp/references/clean-architecture-structure.md`
   — Solution folder layout and layer rules.

3. `.github/skills/firstbank-csharp/references/oracle-script-patterns.md`
   — Idempotent Oracle DDL/DML patterns.

4. Then read **one** of the following based on the task type:
   - **New solution from SDS** → `.github/skills/firstbank-new-project/SKILL.md`
   - **Change to existing solution** → `.github/skills/firstbank-existing-project/SKILL.md`

5. `.github/skills/firstbank-csharp/references/change-report-template.md`
   — Always consult before writing the mandatory change report.

---

## Absolute Rules (Never Negotiable)

| Rule | Requirement |
|------|-------------|
| Database access | Oracle stored procedures **only** — inline SQL is forbidden |
| ORM | Dapper with `CommandType.StoredProcedure` — never EF Core |
| Hashing | SHA-256 only — never SHA-512 |
| Endpoints | HTTPS only — no HTTP exposure |
| Auth | JWT Bearer; `[Authorize]` globally; `[AllowAnonymous]` for Login/HealthCheck only |
| File creation | Manual only — no CLI scaffolding (`dotnet new`, `scaffold`, etc.) |
| Commented-out code | Delete entirely — not permitted in any deployable file |
| Method length | 25–30 lines maximum — refactor if longer |
| Logging | Serilog (preferred) or log4net — never `Console.WriteLine` |
| Oracle user | Dedicated schema user only — never SYS or SYSTEM |

---

## Layer Dependency Rules

```
API → Application → Domain
Infrastructure → Application → Domain

API       ✗→ Infrastructure   (forbidden)
Application ✗→ Infrastructure (forbidden)
Domain    ✗→ anything         (forbidden)
```

Architecture tests in `<SolutionName>.Architecture.Tests/` must enforce these
rules on every build. Do not remove or weaken them.

---

## Naming Conventions (Quick Reference)

| Identifier | Convention | Example |
|-----------|-----------|---------|
| Class | PascalCase | `CustomerRepository` |
| Interface | `I` prefix + PascalCase | `ICustomerRepository` |
| Method | PascalCase | `GetCustomerById` |
| Property | PascalCase | `AccountNumber` |
| Parameter | camelCase | `customerId` |
| Public constant | UPPERCASE | `MAX_RETRY_COUNT` |
| Namespace | `FirstBankNigeria.<SolutionName>.<Layer>.*` | |
| Oracle procedure | `SP_<ACTION>_<ENTITY>` | `SP_GET_CUSTOMER` |
| Oracle package | `PKG_<ENTITY>` | `PKG_CUSTOMER` |
| Oracle table | `<PREFIX>_<NAME>_UPPERCASE` | `FB_CUSTOMERS` |
| Oracle sequence | `SEQ_<TABLE_NAME>` | `SEQ_FB_CUSTOMERS` |

---

## Security Requirements

- All endpoints HTTPS only
- JWT Bearer authentication on every endpoint
- `[Authorize]` at `BaseController` level
- `[AllowAnonymous]` only for Login and HealthCheck endpoints
- SHA-256 for all password hashing — never SHA-512 or plain text
- Usernames stored and compared in lowercase
- Connection strings encrypted in `appsettings.json` — no plain-text secrets
- Maker/Checker controls for every create/update/delete operation
- Audit trail must capture: User ID, Timestamp, IP Address, Activity Type, Activity Details, Comments
- Rate limiting: return `503` + `Retry-After` header when limits are exceeded
- Disable concurrent logins on all web applications

---

## Task Completion Checklist

A task is **not complete** until ALL of the following are produced:

- [ ] All source files organised by Clean Architecture layer (Domain → Application → Infrastructure → API)
- [ ] All Oracle scripts in `/data/scripts/` — idempotent, sequentially numbered
- [ ] Unit tests for every handler
- [ ] Integration tests for every API endpoint
- [ ] Architecture tests verified — all layer dependency rules pass
- [ ] Change report committed to `/docs/reports/YYYY-MM-DD_<short-description>.md`
- [ ] No commented-out code in any file
- [ ] No compiler warnings (except 612, 618, 619, 1030, 1701, 1702)
- [ ] Solution builds with zero errors

---

## PR Description Template

Every pull request opened by this agent must include the following sections:

1. **What was built** — brief summary of the feature or fix
2. **Files changed** — list by Clean Architecture layer
3. **Database changes** — Oracle scripts added or modified
4. **Test coverage** — what tests were added and what they cover
5. **Assumptions made** — any ambiguity in the issue and how it was resolved
6. **Reviewer focus areas** — specific things the reviewer should scrutinise
7. **Change report location** — path to the mandatory report in `/docs/reports/`
