---
name: firstbank-new-project
description: >
  Workflow instructions for initialising and fully building a brand-new FirstBank Nigeria
  solution from an SDS (Software Design Specification). Use this skill whenever a user opens,
  uploads, or references any SDS document — regardless of format (PDF, DOCX, TXT, MD, or any
  other readable file). Triggers for files in docs/specs/ of any format, and for any request
  to scaffold a new solution, build from a spec, or initialise a new Clean Architecture
  project. Also triggers for phrases like "build this from the SDS", "create the solution",
  "set up a new FirstBank project", "here is the SDS", "here is the spec", or "initialise a
  new project". Always read the SDS completely before writing any code, and always consult
  firstbank-csharp skill for all standards. Never scaffold with CLI tools — create all files
  manually.
---

# New Project — Solution Initialisation Workflow

> **Golden rule:** Every decision must comply with the standards in `firstbank-csharp` skill.
> Read that skill **before** writing a single line of code.

---

## Step 1 · Read and Understand the SDS First

Before touching anything, first determine the format of the SDS and read it accordingly:

### 1.1 Handle the SDS Format

| Format | How to handle |
|--------|--------------|
| `.md` / `.txt` | Read directly — content is immediately available |
| `.pdf` | Read the uploaded PDF — Claude can read PDF content natively |
| `.docx` | Read the uploaded Word document — extract all text content |
| Any other format | Ask the user to confirm the content is accessible, or request a copy in a readable format |

> SDS documents are recommended to live in `docs/specs/` regardless of format.
> If the user provides the SDS as an upload or a paste, treat it exactly the same way —
> format does not change the workflow, only how the document is first read.

### 1.2 Extract and Confirm Understanding

Once the SDS is read:

1. Extract and list every **entity**, **use-case**, **API endpoint**, and **database object** described.
2. Note environment-specific requirements (connection strings, external services, config values).
3. Confirm your understanding back to the user with a concise summary before writing any code.
4. Ask clarifying questions for any ambiguity — **do not make silent assumptions**.

---

## Step 2 · Scaffold the Solution

Follow the full layout in `../firstbank-csharp/references/clean-architecture-structure.md`.

**Key reminders:**
- Target **.NET 8.0** / **ASP.NET Core Web API**
- Do **not** put layers inside a `src/` folder
- Create files and directories **manually** — no CLI generators
- Namespace pattern: `FirstBankNigeria.<SolutionName>.<Layer>.*`

---

## Step 3 · Install Mandatory NuGet Packages

Install exactly the packages from `firstbank-csharp` Section 6 (NuGet Packages) for each layer.
No extra packages without written justification.

---

## Step 4 · Implement Layer by Layer (Innermost First)

Work in this strict order: **Domain → Application → Infrastructure → API**

### 4.1 Domain
- Create entities, value objects, enums, domain events, domain exceptions from the SDS
- **Zero external dependencies** — no framework references
- Password fields: store as SHA-256 hash (64-char hex), never SHA-512 or plain text

### 4.2 Application
- One `Features/<FeatureName>/` sub-folder per use-case (vertical slice)
- Each feature folder must contain:
  - `Command.cs` or `Query.cs`
  - `Handler.cs`
  - `Validator.cs` (FluentValidation)
  - `Response.cs` (DTO)
- All repository/service interfaces go in `Common/Interfaces/`
- Register via `DependencyInjection.cs` → `AddApplication(IServiceCollection)`

### 4.3 Infrastructure
- Implement every interface from `Application/Common/Interfaces/`
- All DB access via Oracle stored procedures + Dapper (`CommandType.StoredProcedure`)
- JWT token generation + SHA-256 hashing → `Infrastructure/Identity/`
- Serilog configuration (Thread ID + User ID enrichment) → `Infrastructure/Logging/`
- Register via `DependencyInjection.cs` → `AddInfrastructure(IServiceCollection, IConfiguration)`

### 4.4 API
- Controllers under `Controllers/v1/` — URI versioning: `/v1/...`
- `[Authorize]` at `BaseController`; `[AllowAnonymous]` for Login/HealthCheck only
- Global exception-handling middleware in `Middleware/`
- `launchSettings.json` with `http` + `https` profiles; `"launchUrl": "swagger"`
- Swagger/OpenAPI in `Program.cs` — Bearer security definition + XML comments + full metadata

---

## Step 5 · Database Objects

For every entity/operation from the SDS:

1. Idempotent table DDL + sequence script → `/data/scripts/`
2. Package spec (`PKG_<ENTITY>`) → `/data/scripts/`
3. Package body with stored procedures (`SP_<ACTION>_<ENTITY>`) → `/data/scripts/`
4. Use `SYS_REFCURSOR` for all result sets
5. Number scripts sequentially: `001_...`, `002_...`

See `../firstbank-csharp/references/oracle-script-patterns.md` for full examples.

---

## Step 6 · Security Checklist (Run Before Marking Complete)

- [ ] All endpoints HTTPS-only; no HTTP exposure
- [ ] JWT Bearer configured; `[Authorize]` applied globally
- [ ] Connection strings encrypted in `appsettings.json` — no plain-text secrets
- [ ] Passwords hashed with SHA-256 — never SHA-512 or plain text
- [ ] Maker/Checker controls for all data-mutating operations
- [ ] Audit trail captures: User ID, Timestamp, IP Address, Activity Type, Details, Comments
- [ ] Dedicated Oracle schema user — never SYS or SYSTEM
- [ ] Rate limiting in place; `503` + `Retry-After` when exceeded

---

## Step 7 · Code Quality Checks

- [ ] All methods ≤ 25–30 lines
- [ ] No magic numbers — constants file used
- [ ] No commented-out code
- [ ] All compiler warnings fixed (exceptions: 612, 618, 619, 1030, 1701, 1702)
- [ ] PascalCase for classes/methods/properties; camelCase for parameters
- [ ] No `Console.WriteLine` — Serilog only

---

## Step 8 · Required Output

Do not close the task without producing ALL of the following:

1. **All source files** for every layer, test project, and database script
2. **`launchSettings.json`** for the API project
3. **`.gitignore`** excluding `bin/`, `obj/`, and build artifacts
4. **Change Report** → `/docs/reports/YYYY-MM-DD_<short-description>.md`
   - Use the template in `../firstbank-csharp/references/change-report-template.md`
   - Sections that do not apply → mark as `None`
   - **This report is mandatory — the task is not complete without it**
