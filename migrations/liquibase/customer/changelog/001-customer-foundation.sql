--liquibase formatted sql
--changeset ohs:customer-foundation
CREATE TABLE customers (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL,
    name text NOT NULL,
    email text NOT NULL,
    status text NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    deleted_at timestamptz NULL,
    CONSTRAINT uq_customers_tenant_email UNIQUE (tenant_id, email)
);
CREATE INDEX ix_customers_tenant_status ON customers (tenant_id, status);
