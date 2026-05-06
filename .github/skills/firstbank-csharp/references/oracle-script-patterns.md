# Oracle Script Patterns — Idempotent DDL/DML Reference

> All scripts live in `/data/scripts/`.
> All scripts must be **idempotent** — safe to re-run without error.
> File naming: `NNN_<action>_<object>.sql` (e.g., `001_create_customer_table.sql`)

---

## Table Creation (Idempotent)

```sql
-- 001_create_customer_table.sql
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
  FROM USER_TABLES
  WHERE TABLE_NAME = 'FB_CUSTOMERS';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE '
      CREATE TABLE FB_CUSTOMERS (
        CUSTOMER_ID   NUMBER        NOT NULL,
        FIRST_NAME    VARCHAR2(100) NOT NULL,
        LAST_NAME     VARCHAR2(100) NOT NULL,
        EMAIL         VARCHAR2(200) NOT NULL,
        USERNAME      VARCHAR2(100) NOT NULL,   -- stored in lowercase
        PASSWORD_HASH VARCHAR2(64)  NOT NULL,   -- SHA-256 hex = 64 chars
        IS_ACTIVE     NUMBER(1)     DEFAULT 1   NOT NULL,
        CREATED_BY    VARCHAR2(100) NOT NULL,
        CREATED_AT    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_BY    VARCHAR2(100),
        UPDATED_AT    TIMESTAMP,
        CONSTRAINT PK_FB_CUSTOMERS PRIMARY KEY (CUSTOMER_ID),
        CONSTRAINT UQ_FB_CUSTOMERS_EMAIL UNIQUE (EMAIL),
        CONSTRAINT UQ_FB_CUSTOMERS_USERNAME UNIQUE (USERNAME)
      )
    ';
    DBMS_OUTPUT.PUT_LINE('FB_CUSTOMERS table created.');
  ELSE
    DBMS_OUTPUT.PUT_LINE('FB_CUSTOMERS table already exists — skipped.');
  END IF;
END;
/
```

---

## Sequence Creation (Idempotent)

```sql
-- 002_create_customer_sequence.sql
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count
  FROM USER_SEQUENCES
  WHERE SEQUENCE_NAME = 'SEQ_FB_CUSTOMERS';

  IF v_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_FB_CUSTOMERS START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
    DBMS_OUTPUT.PUT_LINE('SEQ_FB_CUSTOMERS sequence created.');
  ELSE
    DBMS_OUTPUT.PUT_LINE('SEQ_FB_CUSTOMERS sequence already exists — skipped.');
  END IF;
END;
/
```

---

## Package Specification (CREATE OR REPLACE — always idempotent)

```sql
-- 003_pkg_customer_spec.sql
CREATE OR REPLACE PACKAGE PKG_CUSTOMER AS

  -- Retrieve a single customer by ID
  PROCEDURE SP_GET_CUSTOMER(
    p_customer_id IN  NUMBER,
    p_result      OUT SYS_REFCURSOR
  );

  -- Create a new customer
  PROCEDURE SP_CREATE_CUSTOMER(
    p_first_name    IN  VARCHAR2,
    p_last_name     IN  VARCHAR2,
    p_email         IN  VARCHAR2,
    p_username      IN  VARCHAR2,
    p_password_hash IN  VARCHAR2,
    p_created_by    IN  VARCHAR2,
    p_new_id        OUT NUMBER
  );

  -- Update an existing customer
  PROCEDURE SP_UPDATE_CUSTOMER(
    p_customer_id IN  NUMBER,
    p_first_name  IN  VARCHAR2,
    p_last_name   IN  VARCHAR2,
    p_email       IN  VARCHAR2,
    p_updated_by  IN  VARCHAR2,
    p_rows_updated OUT NUMBER
  );

  -- Soft-delete a customer
  PROCEDURE SP_DELETE_CUSTOMER(
    p_customer_id  IN  NUMBER,
    p_deleted_by   IN  VARCHAR2,
    p_rows_updated OUT NUMBER
  );

END PKG_CUSTOMER;
/
```

---

## Package Body (CREATE OR REPLACE — always idempotent)

```sql
-- 004_pkg_customer_body.sql
CREATE OR REPLACE PACKAGE BODY PKG_CUSTOMER AS

  PROCEDURE SP_GET_CUSTOMER(
    p_customer_id IN  NUMBER,
    p_result      OUT SYS_REFCURSOR
  ) AS
  BEGIN
    OPEN p_result FOR
      SELECT CUSTOMER_ID,
             FIRST_NAME,
             LAST_NAME,
             EMAIL,
             USERNAME,
             IS_ACTIVE,
             CREATED_AT
      FROM   FB_CUSTOMERS
      WHERE  CUSTOMER_ID = p_customer_id
      AND    IS_ACTIVE   = 1;
  END SP_GET_CUSTOMER;

  PROCEDURE SP_CREATE_CUSTOMER(
    p_first_name    IN  VARCHAR2,
    p_last_name     IN  VARCHAR2,
    p_email         IN  VARCHAR2,
    p_username      IN  VARCHAR2,
    p_password_hash IN  VARCHAR2,
    p_created_by    IN  VARCHAR2,
    p_new_id        OUT NUMBER
  ) AS
  BEGIN
    SELECT SEQ_FB_CUSTOMERS.NEXTVAL INTO p_new_id FROM DUAL;

    INSERT INTO FB_CUSTOMERS (
      CUSTOMER_ID, FIRST_NAME, LAST_NAME, EMAIL,
      USERNAME, PASSWORD_HASH, CREATED_BY, CREATED_AT
    ) VALUES (
      p_new_id, p_first_name, p_last_name, LOWER(p_email),
      LOWER(p_username), p_password_hash, p_created_by, SYSTIMESTAMP
    );

    COMMIT;
  END SP_CREATE_CUSTOMER;

  PROCEDURE SP_UPDATE_CUSTOMER(
    p_customer_id IN  NUMBER,
    p_first_name  IN  VARCHAR2,
    p_last_name   IN  VARCHAR2,
    p_email       IN  VARCHAR2,
    p_updated_by  IN  VARCHAR2,
    p_rows_updated OUT NUMBER
  ) AS
  BEGIN
    UPDATE FB_CUSTOMERS
    SET    FIRST_NAME = p_first_name,
           LAST_NAME  = p_last_name,
           EMAIL      = LOWER(p_email),
           UPDATED_BY = p_updated_by,
           UPDATED_AT = SYSTIMESTAMP
    WHERE  CUSTOMER_ID = p_customer_id
    AND    IS_ACTIVE   = 1;

    p_rows_updated := SQL%ROWCOUNT;
    COMMIT;
  END SP_UPDATE_CUSTOMER;

  PROCEDURE SP_DELETE_CUSTOMER(
    p_customer_id  IN  NUMBER,
    p_deleted_by   IN  VARCHAR2,
    p_rows_updated OUT NUMBER
  ) AS
  BEGIN
    UPDATE FB_CUSTOMERS
    SET    IS_ACTIVE  = 0,
           UPDATED_BY = p_deleted_by,
           UPDATED_AT = SYSTIMESTAMP
    WHERE  CUSTOMER_ID = p_customer_id
    AND    IS_ACTIVE   = 1;

    p_rows_updated := SQL%ROWCOUNT;
    COMMIT;
  END SP_DELETE_CUSTOMER;

END PKG_CUSTOMER;
/
```

---

## Dapper Repository Pattern (C# side)

```csharp
// All DB calls use stored procedures + OracleParameter — never inline SQL
public async Task<CustomerEntity?> GetByIdAsync(int customerId)
{
    using var connection = new OracleConnection(_connectionString);

    var parameters = new DynamicParameters();
    parameters.Add("p_customer_id", customerId, DbType.Int32, ParameterDirection.Input);
    parameters.Add("p_result", dbType: DbType.Object, direction: ParameterDirection.Output,
                   size: -1 /* OracleDbType.RefCursor */);

    var result = await connection.QueryFirstOrDefaultAsync<CustomerEntity>(
        "PKG_CUSTOMER.SP_GET_CUSTOMER",
        parameters,
        commandType: CommandType.StoredProcedure);

    return result;
}
```

---

## Audit Trail Table (Mandatory for all solutions)

```sql
-- Every solution must include this table. Create idempotently.
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'FB_AUDIT_LOG';
  IF v_count = 0 THEN
    EXECUTE IMMEDIATE '
      CREATE TABLE FB_AUDIT_LOG (
        AUDIT_ID         NUMBER        NOT NULL,
        USER_ID          VARCHAR2(100) NOT NULL,
        IP_ADDRESS       VARCHAR2(50)  NOT NULL,
        ACTIVITY_TYPE    VARCHAR2(100) NOT NULL,
        ACTIVITY_DETAILS VARCHAR2(4000),
        COMMENTS         VARCHAR2(1000),
        CREATED_AT       TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        CONSTRAINT PK_FB_AUDIT_LOG PRIMARY KEY (AUDIT_ID)
      )
    ';
    DBMS_OUTPUT.PUT_LINE(''FB_AUDIT_LOG table created.'');
  ELSE
    DBMS_OUTPUT.PUT_LINE(''FB_AUDIT_LOG table already exists — skipped.'');
  END IF;
END;
/
```
