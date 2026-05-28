# Casita

A small .NET 10 Minimal API for managing home maintenance tickets, organized with a vertical-slice architecture and backed by PostgreSQL via Dapper.

## Project structure

```
Casita.slnx
src/
  Casita.Api/                 # Minimal API host, endpoints, request models
    Features/Tickets/         # Vertical slice: endpoints, service, DTOs
  Casita.Infrastructure/      # Data access, persistence, domain models
    Models/                   # Ticket, Severity
    Persistence/              # Npgsql connection factory, DB initializer
    Tickets/                  # ITicketRepository, TicketRepository
    SqlScripts/               # Schema migrations (run on container init)
compose.yaml                  # Local Postgres + pgAdmin
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or any Docker engine + Compose v2)

## Local testing

The repo ships a `compose.yaml` that spins up everything you need to run the API locally.

### 1. Start the infrastructure

From the repo root:

```bash
docker compose up -d
```

This starts:

| Service  | URL / Port              | Credentials                          |
| -------- | ----------------------- | ------------------------------------ |
| Postgres | `localhost:5432`        | user `postgres` / pass `postgres`, db `casita` |
| pgAdmin  | http://localhost:5050   | `admin@casita.dev` / `admin`         |

On first boot, every `.sql` file in [src/Casita.Infrastructure/SqlScripts](src/Casita.Infrastructure/SqlScripts) is executed automatically against the `casita` database (via Postgres's `/docker-entrypoint-initdb.d` hook).

> Credentials are intentionally weak — **dev only**. Never reuse for any shared/non-local environment.

### 2. Run the API

```bash
dotnet run --project src/Casita.Api
```

The connection string is configured in [appsettings.Development.json](src/Casita.Api/appsettings.Development.json) and points at the Compose Postgres instance. `DatabaseInitializer` runs idempotent schema checks at startup as a safety net.

Test endpoints with [Casita.Api.http](src/Casita.Api/Casita.Api.http) (VS Code REST Client) or `curl`.

### 3. Useful Compose commands

```bash
docker compose logs -f postgres     # tail Postgres logs
docker compose ps                   # check container status
docker compose stop                 # stop containers (keep data)
docker compose down                 # remove containers (keep data volume)
docker compose down -v              # remove containers AND wipe data
```

### Re-seeding the database

The init scripts in `SqlScripts/` only run when the Postgres data volume is **empty**. If you edit a schema file (add a column, new table, new index, etc.) after the DB has already been initialized, the changes will **not** apply on a normal `docker compose up` — `CREATE TABLE IF NOT EXISTS` becomes a no-op, and follow-up statements that depend on new columns will fail with errors like:

```
ERROR: column "created_by" does not exist
```

To reapply after schema changes, wipe the volume and bring it back up:

```bash
docker compose down -v
docker compose up -d
```

> Once the schema stabilizes and we have data worth keeping, we'll switch to a real migration tool (DbUp / FluentMigrator / EF migrations). For now, `down -v` is the workflow.

### Connecting from pgAdmin

1. Open http://localhost:5050 and log in.
2. Add a new server:
   - **Host**: `postgres` (the Compose service name)
   - **Port**: `5432`
   - **Database**: `casita`
   - **Username/Password**: `postgres` / `postgres`

### Testing authenticated endpoints with `dotnet user-jwts`

The API uses JWT bearer authentication (`AddJwtBearer()`), and all `/tickets` endpoints require an authenticated caller. For local development we mint dev tokens with the built-in `dotnet user-jwts` tool — no identity provider needed. This will be replaced by a real IdP later.

> `dotnet user-jwts` stores the signing key in user-secrets (per-project, per-machine) and writes a dev issuer/audience into `appsettings.Development.json` the first time you run it. Nothing is committed to source control.

#### 1. Test personas

To exercise multi-tenant behavior (who can see which tickets) we seed a few fixed homes and members in [002_create_homes.sql](src/Casita.Infrastructure/SqlScripts/002_create_homes.sql) and mint a matching JWT per persona. The `--name` value below is used as the JWT's `sub` claim and lines up with `home_members.user_id`.

| Persona | UserId (`sub`)                           | Home                                       | Role   |
| ------- | ---------------------------------------- | ------------------------------------------ | ------ |
| Alice   | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`   | Casa Uno (`01111111-1111-1111-1111-111111111111`) | owner  |
| Bob     | `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb`   | Casa Uno                                   | member |
| Carol   | `cccccccc-cccc-cccc-cccc-cccccccccccc`   | Casa Dos (`02222222-2222-2222-2222-222222222222`) | owner  |

Alice and Bob share a home, so they should be able to see each other's tickets. Carol lives in a different home and should not see Casa Uno tickets.

Mint a token per persona (run from the repo root):

```bash
dotnet user-jwts create --project src/Casita.Api \
  --name "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  --claim "name=Alice" \
  --scope "tickets:read" --scope "tickets:write"

dotnet user-jwts create --project src/Casita.Api \
  --name "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" \
  --claim "name=Bob" \
  --scope "tickets:read" --scope "tickets:write"

dotnet user-jwts create --project src/Casita.Api \
  --name "cccccccc-cccc-cccc-cccc-cccccccccccc" \
  --claim "name=Carol" \
  --scope "tickets:read" --scope "tickets:write"
```

Each command prints a JWT — copy them into the `@alice_token`, `@bob_token`, and `@carol_token` variables at the top of [Casita.Api.http](src/Casita.Api/Casita.Api.http).

> Homes and memberships are seeded by SQL for now. Adding/joining homes through the API will come later, at which point the persona table moves to real signup flows.

#### 2. Use a token

In [Casita.Api.http](src/Casita.Api/Casita.Api.http), send a request using one of the persona tokens, or use `curl`:

```bash
curl -X POST http://localhost:5220/tickets \
  -H "Authorization: Bearer <alice-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "homeId": "01111111-1111-1111-1111-111111111111",
    "title": "Leaky faucet",
    "description": "Drip drip",
    "severity": 2
  }'
```

Without a valid bearer token you should get a `401 Unauthorized`.

#### Useful `user-jwts` commands

```bash
dotnet user-jwts list --project src/Casita.Api               # all issued tokens
dotnet user-jwts print <id> --project src/Casita.Api --show-all  # decode + claims
dotnet user-jwts remove <id> --project src/Casita.Api        # revoke one
dotnet user-jwts clear --project src/Casita.Api              # revoke all
dotnet user-jwts key --project src/Casita.Api                # view/reset signing key
```

## Build & test

```bash
dotnet build
dotnet test                          # when tests are added
```
