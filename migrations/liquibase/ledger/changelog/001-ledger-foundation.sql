--liquibase formatted sql
--changeset ohs:ledger-foundation
CREATE TABLE ledger_events (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    transaction_id uuid NOT NULL,
    aggregate_type text NOT NULL,
    event_type text NOT NULL,
    schema_version integer NOT NULL,
    payload jsonb NOT NULL,
    version integer NOT NULL,
    occurred_at timestamptz NOT NULL,
    correlation_id uuid NOT NULL,
    CONSTRAINT uq_ledger_events_transaction_version UNIQUE (transaction_id, version)
);
CREATE TABLE ledger_entries_projection (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    transaction_id uuid NOT NULL,
    account_id uuid NOT NULL,
    direction text NOT NULL,
    amount numeric(19,4) NOT NULL,
    currency char(3) NOT NULL,
    created_at timestamptz NOT NULL
);
