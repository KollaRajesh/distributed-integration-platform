--liquibase formatted sql
--changeset ohs:contract-foundation
CREATE TABLE contract_events (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    contract_id uuid NOT NULL,
    aggregate_type text NOT NULL,
    event_type text NOT NULL,
    schema_version integer NOT NULL,
    payload jsonb NOT NULL,
    version integer NOT NULL,
    occurred_at timestamptz NOT NULL,
    causation_id uuid NULL,
    correlation_id uuid NOT NULL,
    CONSTRAINT uq_contract_events_stream_version UNIQUE (contract_id, version)
);
CREATE INDEX ix_contract_events_tenant_stream ON contract_events (tenant_id, contract_id, version);
