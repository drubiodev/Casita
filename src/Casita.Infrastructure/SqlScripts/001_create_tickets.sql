CREATE TABLE IF NOT EXISTS tickets (
    id          UUID PRIMARY KEY,
    home_id     UUID NOT NULL,
    assigned_to UUID NULL,
    title       TEXT NOT NULL,
    description TEXT NOT NULL,
    severity    INTEGER NOT NULL,
    due_date    TIMESTAMPTZ NULL,
    created_at  TIMESTAMPTZ NOT NULL,
    updated_at  TIMESTAMPTZ NOT NULL,
    created_by  TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_tickets_home_id ON tickets (home_id);
CREATE INDEX IF NOT EXISTS ix_tickets_assigned_to ON tickets (assigned_to);
CREATE INDEX IF NOT EXISTS ix_tickets_created_by ON tickets (created_by);
