-- Homes and household membership.
-- For now homes and members are seeded by SQL; later this will move to the API.

CREATE TABLE IF NOT EXISTS homes (
    id         UUID PRIMARY KEY,
    name       TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS home_members (
    home_id   UUID NOT NULL REFERENCES homes(id) ON DELETE CASCADE,
    user_id   UUID NOT NULL,
    role      TEXT NOT NULL DEFAULT 'member' CHECK (role IN ('owner', 'member')),
    joined_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (home_id, user_id)
);

CREATE INDEX IF NOT EXISTS ix_home_members_user_id ON home_members (user_id);

-- ---------------------------------------------------------------------------
-- Seed personas for local testing.
-- Matches the JWTs minted via `dotnet user-jwts create` (see Casita.Api.http).
-- ---------------------------------------------------------------------------

INSERT INTO homes (id, name) VALUES
    ('01111111-1111-1111-1111-111111111111', 'Casa Uno'),
    ('02222222-2222-2222-2222-222222222222', 'Casa Dos')
ON CONFLICT (id) DO NOTHING;

-- Alice + Bob share Casa Uno; Carol lives in Casa Dos.
INSERT INTO home_members (home_id, user_id, role) VALUES
    ('01111111-1111-1111-1111-111111111111',
     'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'owner'),
    ('01111111-1111-1111-1111-111111111111',
     'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'member'),
    ('02222222-2222-2222-2222-222222222222',
     'cccccccc-cccc-cccc-cccc-cccccccccccc', 'owner')
ON CONFLICT (home_id, user_id) DO NOTHING;
