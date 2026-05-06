# Copilot Agent — Skill System

> This directory contains the instruction files and skill definitions that govern how
> the GitHub Copilot coding agent generates code for FirstBank Nigeria solutions.
> Read this document before creating issues, reviewing PRs, or modifying any skill file.

---

## How It Works

```
Issue created (feature.yml or bugfix.yml template)
       ↓
Developer confirms scope + adds copilot-ready label
       ↓
Developer assigns the issue to GitHub Copilot, or starts Copilot from the GitHub Agents panel/page
       ↓
Copilot agent reads copilot-instructions.md (global rules)
       ↓
Copilot agent reads relevant SKILL.md files (ordered)
       ↓
Agent generates code across all Clean Architecture layers
       ↓
CI pipeline runs (build, test, architecture rules)
       ↓
Agent opens draft PR with completed change report
       ↓
Developer reviews PR — role is critic, not author
```

---

## File Map

```
.github/
├── copilot-instructions.md              ← Global agent context. Always loaded.
│
├── instructions/
│   ├── README.md                        ← This file
│   ├── csharp.instructions.md           ← Auto-applied to all *.cs files
│   ├── sql.instructions.md              ← Auto-applied to all *.sql files
│   └── tests.instructions.md            ← Auto-applied to all test files
│
├── skills/
│   ├── firstbank-csharp/
│   │   ├── SKILL.md                     ← Core C# standards (always read first)
│   │   └── references/
│   │       ├── clean-architecture-structure.md
│   │       ├── oracle-script-patterns.md
│   │       └── change-report-template.md
│   │
│   ├── firstbank-new-project/
│   │   └── SKILL.md                     ← Building a new solution from SDS
│   │
│   └── firstbank-existing-project/
│       └── SKILL.md                     ← Modifying an existing solution
│
├── workflows/
│   ├── copilot-agent.yml                ← Validates copilot-ready issue hygiene; does not invoke Copilot
│   └── ci.yml                           ← Build and test on every push/PR
│
└── ISSUE_TEMPLATE/
    ├── config.yml                        ← Disables blank issues
    ├── feature.yml                       ← Feature request template
    └── bugfix.yml                        ← Bug fix template
```

---

## Label Discipline

Labels control what the agent acts on. Use them deliberately.

| Label | Meaning | Who sets it |
|-------|---------|-------------|
| `needs-spec` | Issue exists but is not ready for agent | Auto-set by templates |
| `copilot-ready` | Issue is fully scoped — safe to hand to agent | Developer, manually |
| `human-only` | Auth, migrations, infra — do not assign to agent | Developer, manually |
| `feature` | New feature or enhancement | Auto-set by templates |
| `bug` | Something is broken | Auto-set by templates |

**The `copilot-ready` label is the quality gate.** Only add it when:
- Every field in the issue template is filled
- Acceptance criteria are specific and testable
- Out-of-scope items are explicitly listed
- The task does not touch auth middleware, migrations, or Oracle schema users

After adding `copilot-ready`, start the official GitHub Copilot cloud agent by
assigning the issue to Copilot or by using the GitHub Agents panel/page. GitHub
Actions should validate and test the result, not invoke Copilot directly.

---

## Model Selection Guide

| Task type | Model |
|-----------|-------|
| CRUD endpoints | Sonnet |
| Standard feature (single use-case slice) | Sonnet |
| Bug fix (isolated) | Sonnet |
| Test writing | Sonnet |
| Documentation / change report | Sonnet |
| Complex business logic | Opus |
| Multi-service or multi-layer refactor | Opus |
| Architectural design decisions | Opus |
| Anything touching security or auth patterns | Opus + human review |

---

## Skill Loading Order

The agent always reads skill files in this order:

1. `.github/skills/firstbank-csharp/SKILL.md` — Core C# standards (always, for every task)
2. `.github/skills/firstbank-csharp/references/clean-architecture-structure.md`
3. `.github/skills/firstbank-csharp/references/oracle-script-patterns.md`
4. `.github/skills/firstbank-new-project/SKILL.md` OR `.github/skills/firstbank-existing-project/SKILL.md` (task-dependent)
5. `.github/skills/firstbank-csharp/references/change-report-template.md` (before writing the report)

---

## Developer Review Checklist

When reviewing a Copilot-generated PR, check:

- [ ] All DB access uses stored procedures — no inline SQL anywhere
- [ ] Dapper called with `CommandType.StoredProcedure`
- [ ] SHA-256 used for any hashing — not SHA-512
- [ ] No `Console.WriteLine` — Serilog only
- [ ] No commented-out code
- [ ] Methods are ≤ 30 lines
- [ ] Layer dependencies respected (check architecture test results)
- [ ] Oracle scripts are idempotent and sequentially numbered
- [ ] Change report is present in `/docs/reports/`
- [ ] Unit tests cover the handler; integration tests cover the endpoint
- [ ] Audit trail recorded for any data-mutating operation
- [ ] No new NuGet packages without written justification

---

## Adding or Updating Skill Files

If a standard changes (new NuGet package approved, new naming rule, etc.):

1. Update the relevant SKILL.md or reference file
2. Update `copilot-instructions.md` if it affects the Quick Reference table
3. Update the relevant `.instructions.md` scoped file if it affects file-type rules
4. Open a PR titled `chore: update [skill-name] — [what changed]`
5. Tag a senior engineer for review before merging to `develop`

Skill files are live configuration — they affect every future agent run immediately
upon merge. Treat them with the same care as production code.
