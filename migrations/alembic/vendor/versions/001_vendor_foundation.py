"""Create the Vendor foundation tables."""

from alembic import op
import sqlalchemy as sa

revision = "001_vendor_foundation"
down_revision = None
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "Vendors",
        sa.Column("Id", sa.Uuid(), primary_key=True),
        sa.Column("TenantId", sa.Uuid(), nullable=False),
        sa.Column("Name", sa.String(200), nullable=False),
        sa.Column("ContactEmail", sa.String(320), nullable=False),
        sa.Column("Status", sa.String(30), nullable=False),
        sa.Column("CreatedAt", sa.DateTime(), nullable=False),
        sa.Column("UpdatedAt", sa.DateTime(), nullable=False),
    )


def downgrade() -> None:
    op.drop_table("Vendors")
