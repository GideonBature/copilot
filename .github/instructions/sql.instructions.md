---
applyTo: "**/*.sql,**/data/scripts/**"
---

# FirstBank Nigeria — Oracle SQL Standards

Every `.sql` file in this repository must comply with the following rules.
These are derived from `.github/skills/firstbank-csharp/references/oracle-script-patterns.md`.
Read that file in full before writing any script.

---

## Non-Negotiables

- **All scripts must be idempotent** — safe to re-run multiple times without error
- **All scripts live in `/data/scripts/`** — no SQL anywhere else in the repository
- **File naming**: `NNN_<action>_<object>.sql` (e.g. `001_create_customer_table.sql`)
- **Number sequentially** — never reuse or skip numbers
- **Never edit an existing script in place** — add a new numbered script for changes
- **Dedicated Oracle schema user only** — never SYS or SYSTEM

---

## Idempotency Patterns

### Tables — wrap in DECLARE/BEGIN/END

```sql
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'FB_TABLE_NAME';
  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE FB_TABLE_NAME ( ... )';
    DBMS_OUTPUT.PUT_LINE('FB_TABLE_NAME created.');
  ELSE
    DBMS_OUTPUT.PUT_LINE('FB_TABLE_NAME already exists — skipped.');
  END IF;
END;
/
```

### Sequences — wrap in DECLARE/BEGIN/END

```sql
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM USER_SEQUENCES WHERE SEQUENCE_NAME = 'SEQ_TABLE_NAME';
  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_TABLE_NAME START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
  END IF;
END;
/
```

### Packages and Package Bodies — always CREATE OR REPLACE (inherently idempotent)

```sql
CREATE OR REPLACE PACKAGE PKG_ENTITY AS
  -- procedure declarations
END PKG_ENTITY;
/

CREATE OR REPLACE PACKAGE BODY PKG_ENTITY AS
  -- procedure implementations
END PKG_ENTITY;
/
```

---

## Naming Conventions

| Object | Convention | Example |
|--------|-----------|---------|
| Table | `<PREFIX>_<NAME>_UPPERCASE` | `FB_CUSTOMERS` |
| Sequence | `SEQ_<TABLE_NAME>` | `SEQ_FB_CUSTOMERS` |
| Package | `PKG_<ENTITY>` | `PKG_CUSTOMER` |
| Stored procedure | `SP_<ACTION>_<ENTITY>` | `SP_GET_CUSTOMER` |
| Primary key constraint | `PK_<TABLE_NAME>` | `PK_FB_CUSTOMERS` |
| Unique constraint | `UQ_<TABLE_NAME>_<COLUMN>` | `UQ_FB_CUSTOMERS_EMAIL` |

---

## Mandatory Column Standards

Every table must include these audit columns:

```sql
CREATED_BY    VARCHAR2(100) NOT NULL,
CREATED_AT    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
UPDATED_BY    VARCHAR2(100),
UPDATED_AT    TIMESTAMP
```

Soft-delete pattern — never hard-delete rows:

```sql
IS_ACTIVE     NUMBER(1)     DEFAULT 1 NOT NULL
```

Password hashes: `VARCHAR2(64)` — SHA-256 hex is always 64 characters.
Usernames and emails: always stored via `LOWER()`.

---

## Result Sets

All result-set outputs must use `SYS_REFCURSOR`:

```sql
PROCEDURE SP_GET_ENTITY(
  p_entity_id IN  NUMBER,
  p_result    OUT SYS_REFCURSOR
) AS
BEGIN
  OPEN p_result FOR
    SELECT ... FROM FB_TABLE WHERE ENTITY_ID = p_entity_id AND IS_ACTIVE = 1;
END SP_GET_ENTITY;
```

---

## Audit Log Table

Every solution must have an `FB_AUDIT_LOG` table. Create it idempotently
in `/data/scripts/` with these mandatory columns:
`AUDIT_ID`, `USER_ID`, `IP_ADDRESS`, `ACTIVITY_TYPE`, `ACTIVITY_DETAILS`,
`COMMENTS`, `CREATED_AT`.
