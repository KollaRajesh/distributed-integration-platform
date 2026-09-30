# Distributed integration platform

Distributed integration platform is a .NET Aspire solution for seven independently deployable domain services:

- Customer API: ASP.NET Core, CQRS, PostgreSQL, Liquibase.
- Contract API: ASP.NET Core, CQRS, PostgreSQL, Liquibase.
- Invoice API: ASP.NET Core, CQRS, SQL Server, EF Core code first.
- Payment API: FastAPI, SQLAlchemy, PostgreSQL, Liquibase.
- Vendor API: FastAPI, SQLAlchemy, SQL Server, Alembic.
- Ledger API: ASP.NET Core, CQRS, PostgreSQL, Liquibase.
- Notification API: ASP.NET Core, background processing, PostgreSQL, Liquibase.

The Aspire AppHost runs the services and local infrastructure. RabbitMQ handles asynchronous integration events. Keycloak provides identity
tokens for local, CI, and initial shared development. Microsoft Entra External ID remains a future Azure option.

Contract is the root commercial object. It defines pricing, terms, billing cycles, payment obligations, renewal, cancellation, discounts,
and usage rules. Invoice and Payment depend on Contract through their owned identifiers. Ledger records double-entry financial facts, and
Notification owns delivery workflows. Usage Metering remains optional and deferred.

## Deployment targets

- Local development: .NET Aspire, Docker, Keycloak, PostgreSQL, SQL Server, and RabbitMQ.
- Free cloud demo: Oracle Cloud Always Free with k3s for stateless APIs and lightweight infrastructure.
- Frontend or documentation: Vercel.
- Future paid production: Azure Container Apps for APIs and workers.

## Plan and traceability

The implementation plan is maintained locally in `.plans/ohs-api-implementation-plan.md`. It defines the architecture, phase order,
contracts, event workflows, and delivery decisions.

The plan's requirements traceability matrix maps stable requirement IDs to implementation phases, GitHub Issues, acceptance criteria, and
tests.

Pre-implementation decisions and contracts are in:

- `docs/pre-implementation/cross-cutting-standards.md`
- `docs/pre-implementation/api-contract-baseline.md`
- `docs/pre-implementation/event-contracts-and-readiness.md`
- `docs/pre-implementation/database-schema-baseline.md`
- `docs/pre-implementation/entity-relationship-model.md`
- `docs/pre-implementation/command-event-catalog.md`

## Implementation order

```text
Pre-implementation preparation
    -> Phase 0: Repository and shared foundation
    -> Phase 1: Domain API implementation
    -> Phase 2: Aspire orchestration and databases
    -> Phase 3: Testing and quality
    -> Phase 4: MCP integration
```

## Current status

Phase 0 foundation work is implemented on the `phase-0-foundation` branch. The solution, shared .NET and Python packages, authentication
policies, migration projects, and CI validation are in place.

Build and test locally with:

```text
dotnet build DistributedIntegrationPlatform.slnx --configuration Release
dotnet test DistributedIntegrationPlatform.slnx --configuration Release
python scripts/validate_migrations.py
python scripts/validate_ci.py
```

## Security

APIs validate broker-issued JWT bearer tokens. They do not implement login, issue tokens, store passwords, or act as an identity broker. Do
not commit secrets, tokens, connection strings, or personal data.
