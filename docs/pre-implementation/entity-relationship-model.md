# Entity relationship model

This document defines the logical relationships between the seven initial services and the physical relationships inside each service database.

Each service owns its database. Relationships between services use identifiers and versioned integration events. They are not implemented as cross-database foreign keys or shared tables.

## Domain relationship model

Contract is the root commercial object. Customer and Vendor are reference subjects for a contract. Contract produces invoices. Payments settle invoices. Ledger records accounting facts from invoices and payments. Notification records delivery work triggered by invoice and payment events.

```mermaid
erDiagram
    CUSTOMER ||--o{ CONTRACT : "customerId"
    VENDOR o|--o{ CONTRACT : "vendorId"
    CONTRACT ||--o{ INVOICE : generates
    CUSTOMER ||--o{ INVOICE : billed
    VENDOR o|--o{ INVOICE : "AP invoice"
    INVOICE ||--o{ PAYMENT : settled_by
    CUSTOMER ||--o{ PAYMENT : pays
    VENDOR o|--o{ PAYMENT : receives
    INVOICE ||--o{ LEDGER_TRANSACTION : posts
    PAYMENT ||--o{ LEDGER_TRANSACTION : posts
    INVOICE ||--o{ NOTIFICATION : triggers
    PAYMENT ||--o{ NOTIFICATION : triggers
```

The diagram is logical. `customerId`, `vendorId`, `contractId`, `invoiceId`, and `paymentId` are validated through APIs, projections, and events. They are not cross-service database constraints.

## Service database ownership

| Service | Database | Primary model | Migration |
|---|---|---|---|
| Customer | PostgreSQL | State-based | Liquibase |
| Vendor | SQL Server | State-based | Alembic with SQLAlchemy |
| Contract | PostgreSQL | Event sourced with projections | Liquibase |
| Invoice | SQL Server | State-based with immutable issued documents | EF Core |
| Payment | PostgreSQL | Event sourced with projections | Liquibase |
| Ledger | PostgreSQL | Event sourced with double-entry projections | Liquibase |
| Notification | PostgreSQL | State-based delivery workflow | Liquibase |

## Customer database

```mermaid
erDiagram
    CUSTOMERS ||--o{ CUSTOMER_METADATA : has

    CUSTOMERS {
        uuid id PK
        uuid tenant_id
        text name
        text email
        text phone
        jsonb billing_address
        jsonb tax_profile
        text status
        timestamptz created_at
        timestamptz updated_at
        timestamptz deleted_at
    }

    CUSTOMER_METADATA {
        uuid id PK
        uuid customer_id FK
        text key
        text value
        timestamptz created_at
    }
```

Customer owns identity, contact, billing, tax, and KYC profile data. It does not store contracts, invoices, or payments.

## Vendor database

```mermaid
erDiagram
    VENDORS ||--o{ VENDOR_BANK_ACCOUNTS : owns

    VENDORS {
        uniqueidentifier id PK
        uniqueidentifier tenant_id
        nvarchar name
        nvarchar contact_email
        nvarchar tax_id
        nvarchar status
        datetime2 created_at
        datetime2 updated_at
        datetime2 deleted_at
    }

    VENDOR_BANK_ACCOUNTS {
        uniqueidentifier id PK
        uniqueidentifier vendor_id FK
        varbinary account_number_encrypted
        varbinary routing_number_encrypted
        nvarchar bank_name
        bit verified
        datetime2 created_at
        datetime2 updated_at
    }
```

Bank and routing numbers are encrypted before persistence. Encryption keys are supplied by environment-specific secret management.

## Contract database

Contract is event sourced. `CONTRACT_EVENTS` is the source of truth. Snapshots and projections can be deleted and rebuilt.

```mermaid
erDiagram
    CONTRACT_EVENTS ||--o{ CONTRACT_SNAPSHOTS : snapshots
    CONTRACT_EVENTS ||--o{ CONTRACTS_PROJECTION : projects
    CONTRACTS_PROJECTION ||--o{ CONTRACT_LINE_ITEMS_PROJECTION : contains
    CONTRACT_EVENTS ||--o{ CONTRACT_OUTBOX : publishes

    CONTRACT_EVENTS {
        uuid id PK
        uuid tenant_id
        uuid contract_id
        text aggregate_type
        text event_type
        int schema_version
        jsonb payload
        int version
        timestamptz occurred_at
        uuid causation_id
        uuid correlation_id
    }

    CONTRACT_SNAPSHOTS {
        uuid id PK
        uuid tenant_id
        uuid contract_id
        jsonb snapshot
        int version
        timestamptz created_at
    }

    CONTRACTS_PROJECTION {
        uuid contract_id PK
        uuid tenant_id
        uuid customer_id
        uuid vendor_id
        char currency
        text billing_cycle
        date start_date
        date end_date
        text renewal_type
        text status
        int aggregate_version
        timestamptz updated_at
    }

    CONTRACT_LINE_ITEMS_PROJECTION {
        uuid id PK
        uuid contract_id FK
        uuid tenant_id
        text name
        text description
        numeric unit_price
        numeric quantity
        text billing_mode
        timestamptz updated_at
    }

    CONTRACT_OUTBOX {
        uuid id PK
        uuid tenant_id
        uuid aggregate_id
        text event_type
        int schema_version
        jsonb payload
        timestamptz occurred_at
        timestamptz published_at
        int attempt_count
        text last_error
    }
```

`(contract_id, version)` is unique in the event stream. `customer_id` and `vendor_id` are logical references to Customer and Vendor services.

## Invoice database

Issued invoice values and line items are immutable. Payment status is a derived value updated from payment events.

```mermaid
erDiagram
    INVOICES ||--|{ INVOICE_LINE_ITEMS : contains
    INVOICES ||--o{ INVOICE_EVENTS : records

    INVOICES {
        uniqueidentifier id PK
        uniqueidentifier tenant_id
        uniqueidentifier contract_id
        uniqueidentifier customer_id
        uniqueidentifier vendor_id
        nvarchar invoice_number UK
        decimal amount_due
        decimal amount_paid
        char currency
        datetime2 issue_date
        datetime2 due_date
        nvarchar status
        varbinary document_hash
        datetime2 created_at
        datetime2 updated_at
        datetime2 voided_at
    }

    INVOICE_LINE_ITEMS {
        uniqueidentifier id PK
        uniqueidentifier invoice_id FK
        nvarchar description
        decimal unit_price
        decimal quantity
        decimal total
        datetime2 created_at
    }

    INVOICE_EVENTS {
        uniqueidentifier id PK
        uniqueidentifier invoice_id
        uniqueidentifier tenant_id
        nvarchar event_type
        int schema_version
        nvarchar event_payload
        datetime2 occurred_at
        uniqueidentifier correlation_id
        datetime2 created_at
    }
```

`(tenant_id, invoice_number)` is unique. `contract_id`, `customer_id`, and `vendor_id` are logical references to other service databases.

## Payment database

Payment is event sourced. The original payment stream is never rewritten. Refunds and disputes append new events and create related projection records.

```mermaid
erDiagram
    PAYMENT_EVENTS ||--o{ PAYMENTS_PROJECTION : projects
    PAYMENTS_PROJECTION ||--o{ PAYMENT_REFUNDS_PROJECTION : refunds
    PAYMENT_EVENTS ||--o{ PAYMENT_OUTBOX : publishes

    PAYMENT_EVENTS {
        uuid id PK
        uuid tenant_id
        uuid payment_id
        text aggregate_type
        text event_type
        int schema_version
        jsonb payload
        int version
        timestamptz occurred_at
        uuid causation_id
        uuid correlation_id
    }

    PAYMENTS_PROJECTION {
        uuid payment_id PK
        uuid tenant_id
        uuid invoice_id
        uuid customer_id
        uuid vendor_id
        numeric amount
        char currency
        text method
        text status
        int aggregate_version
        text idempotency_key UK
        timestamptz updated_at
    }

    PAYMENT_REFUNDS_PROJECTION {
        uuid id PK
        uuid tenant_id
        uuid payment_id FK
        numeric amount
        text reason
        text status
        timestamptz created_at
    }

    PAYMENT_OUTBOX {
        uuid id PK
        uuid tenant_id
        uuid aggregate_id
        text event_type
        int schema_version
        jsonb payload
        timestamptz occurred_at
        timestamptz published_at
        int attempt_count
        text last_error
    }
```

`(payment_id, version)` is unique in the event stream. `(tenant_id, idempotency_key)` is unique for payment commands.

## Ledger database

Ledger is event sourced and uses one row per posting. A transaction must balance debits and credits.

```mermaid
erDiagram
    LEDGER_ACCOUNTS ||--o{ LEDGER_ENTRIES_PROJECTION : receives
    LEDGER_EVENTS ||--o{ LEDGER_ENTRIES_PROJECTION : projects

    LEDGER_EVENTS {
        uuid id PK
        uuid tenant_id
        uuid transaction_id
        text aggregate_type
        text event_type
        int schema_version
        jsonb payload
        int version
        timestamptz occurred_at
        uuid causation_id
        uuid correlation_id
    }

    LEDGER_ACCOUNTS {
        uuid id PK
        uuid tenant_id
        text name
        text type
        char currency
        text status
        timestamptz created_at
    }

    LEDGER_ENTRIES_PROJECTION {
        uuid id PK
        uuid tenant_id
        uuid transaction_id
        uuid account_id FK
        text direction
        numeric amount
        char currency
        text reference_type
        uuid reference_id
        timestamptz created_at
    }
```

Valid account types are `asset`, `liability`, `equity`, `revenue`, and `expense`. Valid directions are `debit` and `credit`. `reference_id` points logically to an invoice or payment.

## Notification database

Notification owns delivery workflow state and provider attempts. It does not own customer identity or payment state.

```mermaid
erDiagram
    NOTIFICATIONS ||--o{ NOTIFICATION_ATTEMPTS : attempts
    NOTIFICATIONS ||--o{ NOTIFICATION_OUTBOX : publishes

    NOTIFICATIONS {
        uuid id PK
        uuid tenant_id
        text entity_type
        uuid entity_id
        text channel
        text recipient
        text status
        jsonb payload
        text idempotency_key UK
        timestamptz created_at
        timestamptz updated_at
        timestamptz sent_at
    }

    NOTIFICATION_ATTEMPTS {
        uuid id PK
        uuid notification_id FK
        int attempt_number
        text status
        text provider_message_id
        jsonb response
        timestamptz created_at
    }

    NOTIFICATION_OUTBOX {
        uuid id PK
        uuid tenant_id
        uuid aggregate_id
        text event_type
        int schema_version
        jsonb payload
        timestamptz occurred_at
        timestamptz published_at
        int attempt_count
        text last_error
    }
```

Add a unique constraint on `(notification_id, attempt_number)`. Provider credentials and access tokens must not be stored in notification payloads or attempt responses.

## Relationship and integrity rules

- A service may use foreign keys only within its own database.
- Cross-service IDs are validated through commands, projections, or event consumers.
- Event-sourced aggregates enforce a unique `(aggregate_id, version)` sequence.
- State-based aggregates use optimistic concurrency or a row version.
- Outbox publication occurs in the same transaction as the owning state or event-store write.
- Consumers persist `event_id` in an inbox or consumer-specific deduplication table.
- Projections include the source event position and can be rebuilt.
- Ledger postings must balance within each transaction and currency.
- Financial records are corrected with compensating events, not destructive updates.
- Monetary columns use `numeric(19,4)` or `decimal(19,4)`. Quantities may use a separate higher-precision numeric type.
- Tenant identifiers are required on all tenant-scoped records and indexes.
