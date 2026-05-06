# Change Report Template

> Save reports to: `/docs/reports/YYYY-MM-DD_<short-description>.md`
> This file is **mandatory**. A task is not complete until the report is committed alongside the code.
> Sections that do not apply must be marked as `None` — do not delete them.

---

Copy everything below this line into a new file at `/docs/reports/YYYY-MM-DD_<short-description>.md`
and fill in every section before closing the task.

---

# Change Report — [Short Description]

**Date:** YYYY-MM-DD
**Author:** [Copilot / AI Agent / Developer Name]
**Version:** [Application version after changes]

---

## Summary

[Brief overview of what was implemented or changed — 2 to 4 sentences.]

---

## Features Implemented

- **[Feature 1]**: [Description of what it does and why it was added]
- **[Feature 2]**: [Description]

---

## Files Modified

### Domain Layer
- `FirstBankNigeria.<SolutionName>.Domain/Entities/CustomerEntity.cs` — Created
- *(or "None")*

### Application Layer
- `FirstBankNigeria.<SolutionName>.Application/Features/Customers/Commands/CreateCustomer/CreateCustomerCommand.cs` — Created
- *(or "None")*

### Infrastructure Layer
- `FirstBankNigeria.<SolutionName>.Infrastructure/Persistence/Repositories/CustomerRepository.cs` — Modified
- *(or "None")*

### API Layer
- `FirstBankNigeria.<SolutionName>.Api/Controllers/v1/CustomersController.cs` — Created
- *(or "None")*

---

## Database Changes

- `PKG_CUSTOMER` — Added `SP_CREATE_CUSTOMER` procedure to package body
- `data/scripts/002_add_sp_create_customer.sql` — New idempotent script added
- *(or "None")*

---

## NuGet Packages

| Package | Action | Justification |
|---------|--------|---------------|
| `MediatR` | Added | Required for CQRS command dispatching in Application layer |
| *(or "None")* | | |

---

## Configuration Changes

- `appsettings.json` — Added encrypted `OracleConnectionString` key
- `launchSettings.json` — Added `https` profile with `"launchUrl": "swagger"`
- *(or "None")*

---

## API Endpoint Changes

| Method | Route | Action | Description |
|--------|-------|--------|-------------|
| POST | `/v1/customers` | New | Creates a new customer record |
| GET | `/v1/customers/{id}` | New | Retrieves a customer by ID |
| *(or "None")* | | | |

---

## Breaking Changes

- [Describe any backwards-incompatible changes that affect existing consumers]
- *(or "None")*

---

## Testing Summary

- Unit tests added for `CreateCustomerCommandHandler` — 5 test cases (all passing)
- Integration test added for `POST /v1/customers` — covers happy path and validation failure
- Architecture tests verified — all layer dependency rules pass
- *(or "None")*

---

## Known Issues / Limitations

- [List any technical debt, known bugs, or limitations introduced]
- *(or "None")*

---

## Build Verification

- **Build Status:** Pass ✅ *(or Fail ❌ with explanation)*
- **Test Results:** All passing ✅ *(or list failures)*
- **Architecture Tests:** All passing ✅
- **Warnings:** None *(or list any warnings with justification for each)*
