# Migration foundations

Each service owns its migration directory and database. PostgreSQL services use Liquibase,
Vendor uses Alembic with SQLAlchemy, and Invoice uses EF Core migrations.

Migration files must be reviewed with their owning service. Cross-service identifiers are
logical references only and must not become foreign keys.
