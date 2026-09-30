--liquibase formatted sql
--changeset ohs:payment-foundation
CREATE TABLE payment_events (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    payment_id uuid NOT NULL,
    aggregate_type text NOT NULL,
    event_type text NOT NULL,
    schema_version integer NOT NULL,
    payload jsonb NOT NULL,
    version integer NOT NULL,
    occurred_at timestamptz NOT NULL,
    causation_id uuid NULL,
    correlation_id uuid NOT NULL,
    CONSTRAINT uq_payment_events_stream_version UNIQUE (payment_id, version)
);
CREATE TABLE payments_projection (
    payment_id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    invoice_id uuid NOT NULL,
    amount numeric(19,4) NOT NULL,
    currency char(3) NOT NULL,
    status text NOT NULL,
    idempotency_key text NOT NULL,
    updated_at timestamptz NOT NULL,
    CONSTRAINT uq_payments_tenant_idempotency UNIQUE (tenant_id, idempotency_key)
);
