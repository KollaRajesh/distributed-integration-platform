from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
LIQUIBASE_SERVICES = ("customer", "contract", "payment", "ledger", "notification")


def fail(message: str) -> None:
    print(f"migration validation failed: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> None:
    for service in LIQUIBASE_SERVICES:
        master = ROOT / "migrations" / "liquibase" / service / "db.changelog-master.yaml"
        changelogs = ROOT / "migrations" / "liquibase" / service / "changelog"
        if not master.is_file():
            fail(f"missing Liquibase master for {service}")
        files = sorted(changelogs.glob("*.sql"))
        if not files:
            fail(f"missing Liquibase changeset for {service}")
        for changeset in files:
            text = changeset.read_text(encoding="utf-8")
            if "--liquibase formatted sql" not in text or "--changeset " not in text:
                fail(f"{changeset} is not a formatted Liquibase changeset")

    alembic = ROOT / "migrations" / "alembic" / "vendor"
    if not (alembic / "alembic.ini").is_file() or not list((alembic / "versions").glob("*.py")):
        fail("Vendor Alembic project is incomplete")

    invoice = ROOT / "src" / "InvoiceApi" / "Migrations"
    migration_files = list(invoice.glob("*_*.cs"))
    if not migration_files or not any(
        re.search(r"\[Migration\(\"[^\"]+\"\)\]", file.read_text(encoding="utf-8"))
        for file in migration_files
    ):
        fail("Invoice EF Core migration is missing")

    print("migration structure is valid")


if __name__ == "__main__":
    main()
