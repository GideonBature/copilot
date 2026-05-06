---
applyTo: "**/*.cs"
---

# FirstBank Nigeria — C# File Standards

Every `.cs` file in this repository must comply with the following rules.
These are derived from `.github/skills/firstbank-csharp/SKILL.md`.
Read that file in full before making any changes.

---

## Non-Negotiables for Every C# File

- **No inline SQL** — all database access via Oracle stored procedures through Dapper
- **No EF Core** — Dapper with `CommandType.StoredProcedure` only
- **No SHA-512** — SHA-256 for all hashing
- **No `Console.WriteLine`** — Serilog for all logging
- **No commented-out code** — delete dead code entirely
- **No empty catch blocks** — wrap and rethrow as custom exceptions
- **No `catch (Exception)`** — catch specific exceptions only
- **No magic numbers** — use the constants file
- **Methods ≤ 30 lines** — refactor if a change causes growth beyond this

---

## Layer Rules

When editing a file, verify it belongs to the correct layer and respects the
dependency direction:

```
API → Application → Domain
Infrastructure → Application → Domain
```

- `Application` must never reference `Infrastructure`
- `Domain` must never reference anything external
- `API` must never reference `Infrastructure` directly

---

## Naming

- Classes and methods: PascalCase
- Parameters and local variables: camelCase
- Interfaces: `I` prefix + PascalCase (`ICustomerRepository`)
- Constants: UPPERCASE
- Namespaces: `FirstBankNigeria.<SolutionName>.<Layer>.*`
- No underscores in identifiers (constants are the only exception)
- No abbreviations unless universally understood

---

## Security in Code

- Every controller inherits `BaseController` which carries `[Authorize]`
- Use `[AllowAnonymous]` only on Login and HealthCheck actions
- Hash passwords with SHA-256; store as 64-character hex string
- Store and compare usernames in lowercase
- Never log sensitive data (passwords, tokens, account numbers) in plain text
- Use data masking with asterisks for sensitive fields in logs and UI

---

## Data Access Pattern

All repository methods must follow this pattern:

```csharp
public async Task<T> MethodNameAsync(parameters)
{
    using var connection = new OracleConnection(_connectionString);
    var parameters = new DynamicParameters();
    parameters.Add("p_param_name", value, DbType.Type, ParameterDirection.Input);
    // OUT parameters added the same way with ParameterDirection.Output

    var result = await connection.QueryFirstOrDefaultAsync<T>(
        "PKG_ENTITY.SP_ACTION_ENTITY",
        parameters,
        commandType: CommandType.StoredProcedure);

    return result;
}
```

Never write `connection.Execute("SELECT ...")` or any inline SQL.

---

## Logging

Every service and handler must inject `ILogger<T>` and use it:

```csharp
_logger.LogInformation("Retrieving customer {CustomerId}", customerId);
_logger.LogError(ex, "Failed to create customer {Email}", email);
```

Always include Thread ID and User ID context (configured in Serilog enrichers).
Log levels: Fatal → Error → Warning → Info → Debug. See `.github/skills/firstbank-csharp/SKILL.md` Section 5.
