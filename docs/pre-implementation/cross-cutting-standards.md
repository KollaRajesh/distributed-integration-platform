# Cross-cutting standards

## Summary

- APIs validate broker-issued JWT access tokens and do not issue tokens.
- Services use structured logs, correlation IDs, OpenTelemetry, health checks, and Problem Details.
- RabbitMQ carries versioned integration events.
- Each service owns its database and migrations.
- Contract is the root commercial object. Cross-service relationships use identifiers and events, never shared database tables.
- Use DDD bounded contexts, CQRS command/query separation, and event sourcing for Contract, Payment, and Ledger write models.
- Secrets come from environment-specific secret storage.

## Identity

| Setting | Rule |
|---|---|
| Local and CI broker | Keycloak |
| Initial shared broker | Keycloak |
| Future Azure production broker | Microsoft Entra External ID, when an Azure tenant is available |
| API authentication | JWT bearer validation |
| Browser or desktop client | Authorization Code with PKCE |
| Service or MCP client | Client Credentials, unless delegated user context is required |
| Trusted issuer | Broker only |
| Required token data | `iss`, `aud`, `sub`, expiry, scopes, roles, and tenant claim |

Keycloak can federate Google, GitHub, and Microsoft through provider-specific OAuth/OIDC connections and issues the JWT trusted by the APIs. The APIs never receive provider credentials, store passwords, or accept raw provider tokens.

## Authorization

Use policies that combine permissions, tenant scope, and resource ownership.

```text
platform-admin: global
tenant-admin: tenant
operator: tenant
read-only: tenant
service-client: explicitly assigned
```

Every tenant-scoped resource must carry a tenant identifier. The API derives the caller tenant from a trusted token claim and compares it with the resource tenant.

## Reliability targets

| Measure | Baseline |
|---|---:|
| p95 read latency | 300 ms |
| p95 single-resource write latency | 500 ms |
| p99 normal CRUD latency | 1 second |
| Sustained throughput | 50 requests/second per API instance |
| Default page size | 25 |
| Maximum page size | 100 |
| API edge timeout | 10 seconds |
| Default rate limit | 100 requests/minute per user or client |
| Event publish target | Within 2 seconds of transaction commit |

These are initial targets for local and shared-environment validation, not production SLAs.

## Observability

Every request should carry or create `X-Correlation-Id` and propagate W3C trace context. Structured logs include service, route, status, duration, correlation ID, and trace ID. Logs must exclude tokens, authorization headers, passwords, connection strings, payment secrets, and unnecessary personal data.

Health endpoints must separate liveness from readiness. Readiness checks may include databases, RabbitMQ, and required outbound dependencies.

## Data and migrations

| Service | Database | Data access | Migration |
|---|---|---|---|
| Customer | PostgreSQL | .NET repository | Liquibase |
| Contract | PostgreSQL | .NET repository | Liquibase |
| Invoice | SQL Server | EF Core | EF Core code first |
| Payment | PostgreSQL | SQLAlchemy | Liquibase |
| Vendor | SQL Server | SQLAlchemy | Alembic |
| Ledger | PostgreSQL | .NET repository | Liquibase |
| Notification | PostgreSQL | .NET worker/API | Liquibase |

No service reads another service's database. Migrations run before the dependent API starts and are validated in CI. Event-sourced write models are the source of truth; projections are rebuildable query models.

PostgreSQL is the selected database for Contract because the service needs strong transactional integrity for commercial terms and flexible JSONB-backed structures for line items, discounts, usage rules, and renewal metadata. Contract remains independently owned even though Customer and Payment also use PostgreSQL.

Ledger uses a separate PostgreSQL database for immutable double-entry entries, balances, reconciliation, and audit history. Notification uses a separate PostgreSQL database for notification requests, provider delivery attempts, status transitions, idempotency, and retry state. Neither service reads another service's tables.

## CQRS, DDD, and event sourcing

Each bounded context has its own aggregate roots, value objects, domain services, commands, queries, and policies. Commands mutate aggregates through domain behavior. Queries read projections and do not mutate state.

Contract, Payment, and Ledger use event-sourced write models. Their event streams contain immutable, versioned domain events and are protected by aggregate sequence checks. Contract events record commercial lifecycle and term changes. Payment events record initiation, processing, settlement, refund, and dispute decisions. Ledger events record posted entries, reversals, reconciliation, and audit facts.

Customer, Vendor, Invoice, and Notification use state-based write storage with domain events and transactional outbox records unless later evidence requires full event sourcing. This keeps event sourcing focused on commercial and financial histories while preserving the same DDD and CQRS boundaries across all services.

Every event-sourced service also maintains projections for API reads. Projections include a source event position and can be rebuilt. Public integration events use stable contracts and are not a direct exposure of internal event-store schemas.
