-- Row-level security on tickets, scoped by home membership.
-- The API layer already enforces this; RLS is defense-in-depth in case a
-- query forgets a WHERE clause or someone connects ad-hoc with the app role.

-- ---------------------------------------------------------------------------
-- 1. Application role
-- ---------------------------------------------------------------------------
-- The API connects as `casita_api` (non-superuser, no BYPASSRLS). Migrations
-- and ad-hoc admin work continue to use `postgres`.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'casita_api') THEN
        CREATE ROLE casita_api LOGIN PASSWORD 'casita_api';
    END IF;
END
$$;

GRANT CONNECT ON DATABASE casita TO casita_api;
GRANT USAGE ON SCHEMA public TO casita_api;
GRANT SELECT, INSERT, UPDATE, DELETE ON tickets TO casita_api;
GRANT SELECT ON home_members TO casita_api;
GRANT SELECT ON homes TO casita_api;

-- ---------------------------------------------------------------------------
-- 2. Per-request user context
-- ---------------------------------------------------------------------------
-- The API issues `SET LOCAL app.user_id = '<uuid>'` at the start of each
-- request/transaction. Policies read it via current_setting(...).
CREATE OR REPLACE FUNCTION current_user_id() RETURNS uuid
LANGUAGE sql STABLE AS $$
    SELECT NULLIF(current_setting('app.user_id', true), '')::uuid
$$;

GRANT EXECUTE ON FUNCTION current_user_id() TO casita_api;

-- ---------------------------------------------------------------------------
-- 3. RLS policies on tickets
-- ---------------------------------------------------------------------------
ALTER TABLE tickets ENABLE ROW LEVEL SECURITY;
-- FORCE applies RLS even to the table owner; without this, `postgres` bypasses.
ALTER TABLE tickets FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tickets_select ON tickets;
DROP POLICY IF EXISTS tickets_insert ON tickets;
DROP POLICY IF EXISTS tickets_update ON tickets;
DROP POLICY IF EXISTS tickets_delete ON tickets;

-- SELECT: any ticket in a home the caller belongs to.
CREATE POLICY tickets_select ON tickets
FOR SELECT TO casita_api
USING (
    EXISTS (
        SELECT 1 FROM home_members m
        WHERE m.home_id = tickets.home_id
          AND m.user_id = current_user_id()
    )
);

-- INSERT: caller must belong to the target home and own the ticket they create.
CREATE POLICY tickets_insert ON tickets
FOR INSERT TO casita_api
WITH CHECK (
    created_by = current_user_id()::text
    AND EXISTS (
        SELECT 1 FROM home_members m
        WHERE m.home_id = tickets.home_id
          AND m.user_id = current_user_id()
    )
);

-- UPDATE: creator or assignee can modify; can't move the ticket to another home.
CREATE POLICY tickets_update ON tickets
FOR UPDATE TO casita_api
USING (
    created_by = current_user_id()::text
    OR assigned_to = current_user_id()
)
WITH CHECK (
    EXISTS (
        SELECT 1 FROM home_members m
        WHERE m.home_id = tickets.home_id
          AND m.user_id = current_user_id()
    )
);

-- DELETE: only the creator (admin role can be added later).
CREATE POLICY tickets_delete ON tickets
FOR DELETE TO casita_api
USING (created_by = current_user_id()::text);
