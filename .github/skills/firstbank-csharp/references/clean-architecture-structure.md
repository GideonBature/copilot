# Clean Architecture — Solution Structure Reference

> All FirstBank .NET solutions follow this exact layout.
> Do **not** wrap layer projects inside a `src/` folder.
> Create all files and directories **manually** — no CLI scaffolding.

---

## Repository Root Layout

```
<repository-root>/
│
├── FirstBankNigeria.<SolutionName>.Domain/
├── FirstBankNigeria.<SolutionName>.Application/
├── FirstBankNigeria.<SolutionName>.Infrastructure/
├── FirstBankNigeria.<SolutionName>.Api/
│
├── tests/
│   ├── FirstBankNigeria.<SolutionName>.Domain.Tests/
│   ├── FirstBankNigeria.<SolutionName>.Application.Tests/
│   ├── FirstBankNigeria.<SolutionName>.Infrastructure.Tests/
│   ├── FirstBankNigeria.<SolutionName>.Api.Tests/
│   └── FirstBankNigeria.<SolutionName>.Architecture.Tests/
│
├── data/
│   ├── sample/          ← Sample/seed data (CSV, JSON)
│   └── scripts/         ← All Oracle DDL, DML, and seed SQL scripts
│
├── docs/
│   ├── specs/           ← SDS documents (triggers New Project workflow)
│   ├── manuals/         ← User and operational manuals
│   └── reports/         ← Mandatory change reports (YYYY-MM-DD_<desc>.md)
│
├── .gitignore           ← Must exclude bin/, obj/, and build artifacts
└── FirstBankNigeria.<SolutionName>.sln
```

---

## Layer Internal Structure

### Domain Layer
```
Domain/
├── Entities/
├── ValueObjects/
├── Enums/
├── Events/              ← Domain events
├── Exceptions/          ← Domain-specific exceptions
└── Constants/
```
**Rules:** Zero external dependencies. No framework references.

---

### Application Layer
```
Application/
├── Features/
│   └── <FeatureName>/
│       ├── Commands/
│       │   └── <ActionName>/
│       │       ├── <ActionName>Command.cs
│       │       ├── <ActionName>CommandHandler.cs
│       │       ├── <ActionName>CommandValidator.cs   ← FluentValidation
│       │       └── <ActionName>Response.cs
│       └── Queries/
│           └── <QueryName>/
│               ├── <QueryName>Query.cs
│               ├── <QueryName>QueryHandler.cs
│               └── <QueryName>Response.cs
├── Common/
│   ├── Interfaces/      ← All repository/service interfaces defined here
│   ├── Behaviours/      ← MediatR pipeline behaviours (validation, logging)
│   └── Exceptions/      ← Application-level exceptions
└── DependencyInjection.cs
```
**Rules:** References Domain only. Never references Infrastructure.

---

### Infrastructure Layer
```
Infrastructure/
├── Persistence/
│   └── Repositories/    ← Implements interfaces from Application/Common/Interfaces/
├── Identity/            ← JWT generation, SHA-256 password hashing
├── Logging/             ← Serilog configuration, Thread+User enrichment
├── ExternalServices/    ← Third-party integrations
└── DependencyInjection.cs
```
**Rules:** References Application (for interfaces) and Domain. Never referenced by Application.

---

### API Layer
```
Api/
├── Controllers/
│   └── v1/              ← URI versioning — all routes start /v1/
├── Middleware/           ← Global exception handler
├── Extensions/          ← Program.cs extension methods
├── Properties/
│   └── launchSettings.json  ← Must have http + https profiles; launchUrl = "swagger"
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```
**Rules:** References Application only (sends commands/queries via MediatR). Never references Infrastructure directly.

---

## Dependency Direction (Enforced by Architecture Tests)

```
API → Application → Domain
Infrastructure → Application → Domain

API ✗→ Infrastructure   (forbidden)
Application ✗→ Infrastructure   (forbidden)
Domain ✗→ anything   (forbidden)
```

Architecture tests in `<SolutionName>.Architecture.Tests/` enforce these rules on every build.

---

## launchSettings.json Minimum Template

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7000;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## .gitignore Required Entries

```
bin/
obj/
*.user
.vs/
*.suo
publish/
```
