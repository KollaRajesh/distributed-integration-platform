--liquibase formatted sql
--changeset ohs:notification-foundation
CREATE TABLE notifications (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    entity_type text NOT NULL,
    entity_id uuid NOT NULL,
    channel text NOT NULL,
    recipient text NOT NULL,
    status text NOT NULL,
    payload jsonb NOT NULL,
    idempotency_key text NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    CONSTRAINT uq_notifications_tenant_idempotency UNIQUE (tenant_id, idempotency_key)
);
