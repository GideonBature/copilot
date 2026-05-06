---
name: firstbank-existing-project
description: >
  Workflow instructions for implementing changes (new features, bug fixes, enhancements,
  refactors) on an existing FirstBank Nigeria solution. Use this skill whenever a user asks
  to add a feature, fix a bug, update existing code, implement a change request, or modify
  any .cs file in an existing FirstBank solution. Also triggers for phrases like "add this
  to the existing project", "implement this change", "update the handler", "fix this bug",
  "extend the API", or any modification to already-scaffolded Clean Architecture code.
  Always understand the current behaviour before changing anything, and always consult
  firstbank-csharp skill for all standards.
---

# Existing Project — Change Implementation Workflow

> **Golden rule:** Every decision must comply with the standards in `firstbank-csharp` skill.
> Read that skill **before** writing a single line of code.

---

## Step 1 · Understand Before Touching Anything

Before changing a single file:

1. Read the change request, ticket, or SDS delta **in full**
2. Identify every **file**, **class**, **stored procedure**, and **Oracle package** affected
3. Trace the current flow: `Controller → Handler → Repository → Database`
4. Confirm the **scope** of the change — if ambiguous, ask for clarification; never make silent assumptions
5. Check whether the change requires a **new feature branch** (almost certainly yes — see `firstbank-csharp` Section 9)

---

## Step 2 · Respect the Existing Architecture

| Rule | Detail |
|------|--------|
| No layer leakage | Application must never reference Infrastructure. Domain references nothing. |
| New use-cases = new slices | Add `Application/Features/<FeatureName>/` — do not bloat existing handlers |
| New interfaces first | Define in `Application/Common/Interfaces/` before implementing in Infrastructure |
| No inline SQL | All DB access via Oracle stored procedures via Dapper |
| No new frameworks | Do not add NuGet packages not in the approved list without written justification |

---

## Step 3 · Implement the Change (Layer by Layer, Innermost First)

### 3.1 Domain (only if new entities/value objects needed)
- Add entities, value objects, enums, domain events to the Domain project
- Maintain zero external dependencies

### 3.2 Application
- Create or update `Command/Query`, `Handler`, `Validator`, and `Response` in `Features/<FeatureName>/`
- Add or update **interfaces** in `Common/Interfaces/` for any new infrastructure needs
- Update `DependencyInjection.cs` only if new services are being registered

### 3.3 Infrastructure
- Implement any new interfaces from the Application layer
- All stored procedure calls via `OracleParameter` — no concatenated SQL
- Update `DependencyInjection.cs` only if new services are introduced

### 3.4 API
- Add new controllers or action methods; keep URI versioning: `/v1/...`
- Every new or modified endpoint needs full XML doc comments + `[ProducesResponseType]` attributes
- Do **not** remove or rename existing endpoints without flagging as a **breaking change**

---

## Step 4 · Database Changes

For every new or modified database object:

1. Write a new **idempotent** DDL/DML script in `/data/scripts/` — do not edit original scripts in-place
2. For new procedures in an existing package: update the **package body** and keep spec in sync
3. Follow naming: `SP_<ACTION>_<ENTITY>` for procedures, `PKG_<ENTITY>` for packages
4. Use `SYS_REFCURSOR` for result sets
5. Scripts must be safe to re-run without error

See `../firstbank-csharp/references/oracle-script-patterns.md` for patterns.

---

## Step 5 · Security Checklist

- [ ] No new HTTP-only endpoints — HTTPS only
- [ ] New endpoints have `[Authorize]`; `[AllowAnonymous]` only where explicitly justified
- [ ] No secrets in plain text — encrypted in `appsettings.json`
- [ ] Any new password handling uses SHA-256 — not SHA-512
- [ ] Maker/Checker controls added for any new data-mutating operation
- [ ] Audit trail recorded for every new action (User ID, Timestamp, IP, Activity Type, Details)
- [ ] No new direct Oracle SYS/SYSTEM access introduced

---

## Step 6 · Code Quality Checks

- [ ] All methods ≤ 25–30 lines (refactor if change causes growth beyond this)
- [ ] New constants in the existing constants file — not inline
- [ ] No commented-out code introduced or left from prior code
- [ ] No new compiler warnings (unsuppressed)
- [ ] Existing naming conventions followed throughout

---

## Step 7 · Regression Safety

Before finalising:

- [ ] Review all **callers** of any method or interface changed — no unintended breakage
- [ ] Add or update **unit and integration tests** for every changed behaviour
- [ ] Run architecture tests — all layer dependency rules must still pass
- [ ] Confirm the **solution still builds** with zero errors

---

## Step 8 · Required Output

Do not close the task without producing ALL of the following:

1. **All modified/created source files**, organised by Clean Architecture layer
2. **Updated database scripts** in `/data/scripts/` for any new or altered Oracle objects
3. **Change Report** → `/docs/reports/YYYY-MM-DD_<short-description>.md`
   - Use the template in `../firstbank-csharp/references/change-report-template.md`
   - Sections that do not apply → mark as `None`
   - **This report is mandatory — the task is not complete without it**
4. If **breaking changes** are introduced, list them explicitly in the report and flag to the reviewer
