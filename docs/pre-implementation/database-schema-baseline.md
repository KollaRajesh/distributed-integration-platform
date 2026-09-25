# Database schema baseline

Each service owns a separate database. Cross-service identifiers such as `customer_id`, `vendor_id`, `contract_id`, `invoice_id`, and `payment_id` are logical references only. They must not become cross-database foreign keys.

The command and event names, aggregate boundaries, and integration-event consumers are defined in `command-event-catalog.md`. This schema document defines persistence only.

All tables use UTC timestamps. Monetary values use fixed precision, not floating point. Tenant-scoped tables include `tenant_id` unless the deployment is explicitly single-tenant.

## Customer service

Database: PostgreSQL
Persistence: state-based write model with domain events and transactional outbox
Migration: Liquibase

### `customers`

| Column | Type | Rules |
|---|---|---|
| `id` | `uuid` | Primary key |
| `tenant_id` | `uuid` | Required, indexed |
| `name` | `text` | Required |
| `email` | `text` | Required, normalized and indexed per tenant |
| `phone` | `text` | Optional |
| `billing_address` | `jsonb` | Optional, validated at the application boundary |
| `tax_profile` | `jsonb` | Optional, protected from unnecessary logging |
| `status` | `text` | `active` or `inactive` |
| `created_at` | `timestamptz` | Required |
| `updated_at` | `timestamptz` | Required |
| `deleted_at` | `timestamptz` | Optional soft-delete marker |

### `customer_metadata`

`id uuid primary key`, `customer_id uuid`, `key text`, `value text`, and `created_at timestamptz`. Add a unique constraint on `(customer_id, key)` and an index on `customer_id`.

## Vendor service

Database: SQL Server
Persistence: state-based write model with domain events and transactional outbox
Migration: Alembic with SQLAlchemy

### `Vendors`

| Column | Type | Rules |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary key |
| `TenantId` | `uniqueidentifier` | Required, indexed |
| `Name` | `nvarchar(200)` | Required |
| `ContactEmail` | `nvarchar(320)` | Required |
| `TaxId` | `nvarchar(100)` | Protected and encrypted where required |
| `Status` | `nvarchar(30)` | Lifecycle status |
| `CreatedAt` | `datetime2` | UTC |
| `UpdatedAt` | `datetime2` | UTC |
| `DeletedAt` | `datetime2` | Optional soft-delete marker |

### `VendorBankAccounts`

`Id uniqueidentifier primary key`, `VendorId uniqueidentifier` foreign key to `Vendors.Id`, `AccountNumberEncrypted varbinary(max)`, `RoutingNumberEncrypted varbinary(max)`, `BankName nvarchar(200)`, `Verified bit`, `CreatedAt datetime2`, and `UpdatedAt datetime2`.

Encryption keys must come from environment-specific secret management. The database must never contain plaintext bank account or routing numbers.

## Contract service

Database: PostgreSQL
Persistence: event-sourced write model with snapshots, projections, and transactional outbox
Migration: Liquibase

### `contract_events`

| Column | Type | Rules |
|---|---|---|
| `id` | `uuid` | Event identity, primary key |
| `tenant_id` | `uuid` | Required, indexed |
| `contract_id` | `uuid` | Aggregate identity, indexed |
| `aggregate_type` | `text` | `contract` |
| `event_type` | `text` | Versioned domain event name |
| `schema_version` | `integer` | Required |
| `payload` | `jsonb` | Immutable event data |
| `version` | `integer` | Aggregate sequence |
| `occurred_at` | `timestamptz` | Required |
| `causation_id` | `uuid` | Optional |
| `correlation_id` | `uuid` | Required |

Add a unique constraint on `(contract_id, version)` and indexes on `(tenant_id, contract_id, version)` and `(tenant_id, occurred_at)`.

### `contract_snapshots`

`id uuid primary key`, `tenant_id uuid`, `contract_id uuid`, `snapshot jsonb`, `version integer`, and `created_at timestamptz`. Add a unique constraint on `(contract_id, version)` and use the newest valid snapshot only as a replay optimization.

### Projections

`contracts_projection` contains `contract_id`, `tenant_id`, `customer_id`, nullable `vendor_id`, `currency`, `billing_cycle`, `start_date`, `end_date`, `renewal_type`, `status`, `aggregate_version`, and `updated_at`.

`contract_line_items_projection` contains `id`, `contract_id`, `tenant_id`, `name`, `description`, `unit_price numeric(19,4)`, `quantity numeric(19,6)`, `billing_mode`, and `updated_at`.

Projections are rebuildable and are not the source of truth.

### `contract_outbox`

`id uuid primary key`, `tenant_id uuid`, `aggregate_id uuid`, `event_type text`, `schema_version integer`, `payload jsonb`, `occurred_at timestamptz`, `published_at timestamptz`, `attempt_count integer`, and `last_error text`.

Use `published_at is null` for pending records. Do not rely only on a boolean `processed` flag because publication time and retry state are needed for operations.

## Invoice service

Database: SQL Server
Persistence: state-based write model with immutable issued documents and domain events
Migration: EF Core code-first migrations

### `Invoices`

`Id uniqueidentifier primary key`, `TenantId uniqueidentifier`, `ContractId uniqueidentifier`, `CustomerId uniqueidentifier`, nullable `VendorId uniqueidentifier`, `InvoiceNumber nvarchar(80)`, `AmountDue decimal(19,4)`, `AmountPaid decimal(19,4)`, `Currency char(3)`, `IssueDate datetime2`, `DueDate datetime2`, `Status nvarchar(30)`, `DocumentHash varbinary(64)`, `CreatedAt datetime2`, `UpdatedAt datetime2`, and `VoidedAt datetime2`.

Add a unique constraint on `(TenantId, InvoiceNumber)`. After issuance, financial fields and line items are immutable. Payment status is a derived field updated from payment events.

### `InvoiceLineItems`

`Id uniqueidentifier primary key`, `InvoiceId uniqueidentifier` foreign key to `Invoices.Id`, `Description nvarchar(500)`, `UnitPrice decimal(19,4)`, `Quantity decimal(19,6)`, `Total decimal(19,4)`, and `CreatedAt datetime2`.

### `InvoiceEvents`

`Id uniqueidentifier primary key`, `InvoiceId uniqueidentifier`, `TenantId uniqueidentifier`, `EventType nvarchar(200)`, `SchemaVersion int`, `EventPayload nvarchar(max)`, `OccurredAt datetime2`, `CorrelationId uniqueidentifier`, and `CreatedAt datetime2`.

## Payment service

Database: PostgreSQL
Persistence: event-sourced write model with idempotency, projections, and transactional outbox
Migration: Liquibase

### `payment_events`

Use the same event-store fields as Contract: `id`, `tenant_id`, `payment_id`, `aggregate_type`, `event_type`, `schema_version`, `payload`, `version`, `occurred_at`, `causation_id`, and `correlation_id`.

Add a unique constraint on `(payment_id, version)` and indexes on `(tenant_id, payment_id, version)` and `(tenant_id, occurred_at)`.

### `payments_projection`

`payment_id uuid primary key`, `tenant_id uuid`, `invoice_id uuid`, `customer_id uuid`, nullable `vendor_id uuid`, `amount numeric(19,4)`, `currency char(3)`, `method text`, `status text`, `aggregate_version integer`, `idempotency_key text`, and `updated_at timestamptz`.

Add a unique constraint on `(tenant_id, idempotency_key)`.

### `payment_refunds_projection`

`id uuid primary key`, `tenant_id uuid`, `payment_id uuid`, `amount numeric(19,4)`, `reason text`, `status text`, and `created_at timestamptz`.

### `payment_outbox`

Use the standard outbox fields: `id`, `tenant_id`, `aggregate_id`, `event_type`, `schema_version`, `payload`, `occurred_at`, `published_at`, `attempt_count`, and `last_error`.

## Ledger service

Database: PostgreSQL
Persistence: event-sourced write model with double-entry projections
Migration: Liquibase

### `ledger_events`

`id uuid primary key`, `tenant_id uuid`, `transaction_id uuid`, `aggregate_type text`, `event_type text`, `schema_version integer`, `payload jsonb`, `version integer`, `occurred_at timestamptz`, `causation_id uuid`, and `correlation_id uuid`.

Add a unique constraint on `(transaction_id, version)`.

### `ledger_accounts`

`id uuid primary key`, `tenant_id uuid`, `name text`, `type text`, `currency char(3)`, `status text`, and `created_at timestamptz`. Account types are `asset`, `liability`, `equity`, `revenue`, and `expense`.

### `ledger_entries_projection`

Use one row per posting, not one row with both debit and credit columns:

`id uuid primary key`, `tenant_id uuid`, `transaction_id uuid`, `account_id uuid`, `direction text`, `amount numeric(19,4)`, `currency char(3)`, `reference_type text`, `reference_id uuid`, `created_at timestamptz`.

For every posted transaction, the sum of debit amounts must equal the sum of credit amounts. A transaction must contain at least two postings.

## Notification service

Database: PostgreSQL
Persistence: state-based workflow with transactional outbox
Migration: Liquibase

### `notifications`

`id uuid primary key`, `tenant_id uuid`, `entity_type text`, `entity_id uuid`, `channel text`, `recipient text`, `status text`, `payload jsonb`, `idempotency_key text`, `created_at timestamptz`, `updated_at timestamptz`, and `sent_at timestamptz`.

Channels are `email`, `sms`, and `push`. Provider credentials and access tokens are never stored in `payload`.

### `notification_attempts`

`id uuid primary key`, `notification_id uuid`, `attempt_number integer`, `status text`, `provider_message_id text`, `response jsonb`, `created_at timestamptz`.

Add a unique constraint on `(notification_id, attempt_number)`.

### `notification_outbox`

Use the standard outbox fields: `id`, `tenant_id`, `aggregate_id`, `event_type`, `schema_version`, `payload`, `occurred_at`, `published_at`, `attempt_count`, and `last_error`.

## Shared schema rules

- Use UUID or `uniqueidentifier` generated by the owning service.
- Use `numeric(19,4)` or `decimal(19,4)` for money and `char(3)` for ISO currency codes.
- Add tenant-aware indexes to every query path.
- Use optimistic concurrency for state-based aggregates and expected sequence versions for event-sourced aggregates.
- Store event payloads and integration contracts with explicit schema versions.
- Keep outbox publication and the owning write transaction atomic.
- Store consumer deduplication in an inbox table or consumer-specific event table.
- Never use cross-service foreign keys or shared tables.
- Redact financial secrets and personal data from event payloads, logs, and error responses.
