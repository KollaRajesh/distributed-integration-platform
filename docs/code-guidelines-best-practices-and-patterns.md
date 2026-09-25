# Code guidelines, best practices, and patterns

This document defines the coding and architecture baseline for the OHS APIs. Project-specific configuration and established local patterns take precedence when they are intentional and documented.

## Solution structure

Use one solution with an Aspire AppHost and independently deployable services:

```text
src/
  AppHost/
  CustomerApi/
  InvoiceApi/
  PaymentApi/
  VendorApi/
  BuildingBlocks/
tests/
  Unit/
  Integration/
  Contract/
docs/
infrastructure/
.plans/
```

The AppHost owns local orchestration, resource references, dependency ordering, and developer experience. It does not own domain behavior. Shared libraries should contain stable cross-cutting concerns, not service-specific rules.

## Application architecture

Use a small CQRS structure:

```text
Api/
Application/
  Commands/
  Queries/
  Validators/
Domain/
  Entities/
  ValueObjects/
  Rules/
Infrastructure/
  Persistence/
  Migrations/
```

Keep endpoints thin. An endpoint should bind and validate input, call an application handler or service, and translate the result into the documented HTTP response. Domain rules belong in the domain or application layer, not in controllers, route functions, repositories, or database mappings.

Use records or immutable DTOs for request and response models when that matches the language and framework version. Keep persistence models separate from API contracts. Use explicit mapping for fields that affect security, identity, ownership, or soft deletion.

## .NET guidance

- Use nullable reference types and treat warnings as design feedback.
- Use PascalCase for public symbols and camelCase or the established underscore-private-field convention for locals and private fields.
- Name asynchronous methods with the `Async` suffix.
- Prefer guard clauses and specific exception types.
- Preserve exception stack traces with `throw;`.
- Do not block on asynchronous work with `.Result`, `.Wait()`, or `GetAwaiter().GetResult()`.
- Pass cancellation tokens through database, HTTP, and application calls.
- Use `IHttpClientFactory` for outbound HTTP.
- Use options classes with validation rather than scattered raw configuration access.
- Use typed results or `ActionResult<T>` and return status codes that match the contract.
- Centralize exception handling with Problem Details middleware or `IExceptionHandler`.
- Use `ILogger<T>` with structured properties rather than interpolated log messages.
- Keep EF Core configurations explicit and review generated migrations before applying them.

## Python guidance

- Use type hints throughout application and infrastructure code.
- Keep FastAPI route functions thin and use dependency injection for application services, sessions, authentication, and authorization.
- Use Pydantic models for API input and output contracts.
- Keep SQLAlchemy models and database session handling in infrastructure.
- Use async database and HTTP clients consistently when the service is asynchronous.
- Avoid broad exception handlers. Translate only known exceptions and let unexpected failures reach centralized error handling.
- Keep settings in typed configuration models and validate required values at startup.
- Use structured logging with consistent service, route, correlation, trace, and result fields.

## API contract patterns

Use routes such as:

```text
POST   /v1/customers
GET    /v1/customers
GET    /v1/customers/{customerId}
PUT    /v1/customers/{customerId}
DELETE /v1/customers/{customerId}
```

List endpoints should support only documented filters and sort fields. Reject unknown or invalid query values instead of silently ignoring them. Enforce a default and maximum page size. Use stable ordering, especially when paging by a non-unique field.

Return a consistent list envelope:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalCount": 0,
  "hasNextPage": false
}
```

Use RFC 9457-compatible Problem Details with a stable application error code and field-level validation details where applicable. Do not expose implementation details, SQL errors, stack traces, or secrets.

## Persistence patterns

- Each API owns its database schema and migrations.
- Customer and Payment use PostgreSQL with Liquibase.
- Invoice uses SQL Server with EF Core code-first migrations.
- Vendor uses SQL Server with SQLAlchemy and a reviewed migration strategy.
- Apply soft-delete predicates consistently through repositories or query specifications.
- Add indexes for foreign keys, uniqueness constraints, common filters, sort keys, search fields, and date ranges.
- Use transactions around commands that change multiple records.
- Define ownership and referential behavior explicitly. Do not rely on provider-specific defaults.
- Keep seed data deterministic and safe to run repeatedly in local and test environments.

## Authentication and authorization

Validate JWT issuer, audience, signature, lifetime, and required claims. Use shared policy names and equivalent role semantics across .NET and Python services. Separate authentication from authorization:

1. Authenticate the caller.
2. Evaluate the endpoint policy.
3. Evaluate resource ownership or tenant scope where required.
4. Execute the application operation.

Never use feature flags to grant or remove authorization. Do not log tokens or complete claims when they contain personal or security-sensitive data.

## Reliability and observability

Use correlation and trace propagation for inbound and outbound calls. Every request log should include service, route, result status, duration, correlation ID, and trace ID when available.

Define timeout and retry policies per outbound dependency. Retry transient reads and explicitly idempotent operations only. Use circuit breakers where a failing dependency could exhaust service resources. Health endpoints should distinguish liveness from readiness and should report database readiness without exposing credentials.

## MCP patterns

Start with one MCP server per API:

```text
MCP client -> API-specific MCP server -> HTTP API -> application handler -> database
```

MCP servers should:

- expose typed tools with names and input schemas that match the API contract;
- reuse API authentication and authorization;
- pass correlation and trace context;
- map API errors without inventing new business semantics;
- avoid direct database access;
- avoid duplicating validation and domain rules.

A later shared MCP layer may compose the four API-specific servers or call their HTTP APIs. It must preserve the same boundaries.

## Testing patterns

Use Arrange, Act, Assert for unit tests. Name tests by operation, state, and expected behavior, for example `GetCustomer_WhenMissing_ReturnsNotFound`.

Test behavior rather than private implementation details. Cover happy paths, boundaries, invalid input, authorization failures, concurrency-sensitive commands, soft deletion, idempotent upsert, pagination, and dependency failures. Use mocks only for direct abstractions where a test double clarifies the behavior. Prefer real database and distributed-resource integration tests for persistence, migrations, serialization, and service wiring.

## Review checklist

- Does the change preserve the API contract and versioning rules?
- Are nullable and type-safety warnings addressed?
- Are authorization and resource ownership checks explicit?
- Are errors mapped consistently without leaking internals?
- Are retries safe for the operation?
- Are cancellation, timeouts, and resource disposal handled?
- Are migrations and indexes reviewed?
- Are logs structured and free of secrets or unnecessary personal data?
- Are unit, integration, contract, and OpenAPI tests updated where behavior changed?
- Does the change keep domain logic out of AppHost, MCP wrappers, controllers, route functions, and persistence mappings?
