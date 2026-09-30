from alembic import context

target_metadata = None


def run_migrations_online() -> None:
    context.configure(
        url=context.config.get_main_option("sqlalchemy.url"),
        target_metadata=target_metadata,
        compare_type=True,
    )
    with context.begin_transaction():
        context.run_migrations()


run_migrations_online()
