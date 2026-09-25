# Distributed integration platform

Distributed integration platform is a .NET Aspire solution for four independently deployable domain services:

- Customer API: ASP.NET Core, CQRS, PostgreSQL, Liquibase.
- Invoice API: ASP.NET Core, CQRS, SQL Server, EF Core code first.
- Payment API: FastAPI, SQLAlchemy, PostgreSQL, Liquibase.
- Vendor API: FastAPI, SQLAlchemy, SQL Server, Alembic.

The Aspire AppHost runs the services and local infrastructure. RabbitMQ handles asynchronous integration events. Keycloak provides identity tokens for local, CI, and initial shared development. Microsoft Entra External ID remains a future Azure option.

## Plan and traceability

The implementation plan is maintained locally in `.plans/ohs-api-implementation-plan.md`. It defines the architecture, phase order, contracts, event workflows, and delivery decisions.

The plan's requirements traceability matrix maps stable requirement IDs to implementation phases, GitHub Issues, acceptance criteria, and tests.

Pre-implementation decisions and contracts are in:

- `docs/pre-implementation/cross-cutting-standards.md`
- `docs/pre-implementation/api-contract-baseline.md`
- `docs/pre-implementation/event-contracts-and-readiness.md`

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

The repository is being prepared on the `pre-implementation` branch. Phase 0 work will use the `phase-0-foundation` branch.

Build, test, and deployment commands will be added with the initial solution structure.

## Security

APIs validate broker-issued JWT bearer tokens. They do not implement login, issue tokens, store passwords, or act as an identity broker. Do not commit secrets, tokens, connection strings, or personal data.
