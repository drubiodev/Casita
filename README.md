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

The init scripts in `SqlScripts/` only run when the Postgres data volume is **empty**. To reapply them after changes:

```bash
docker compose down -v
docker compose up -d
```

### Connecting from pgAdmin

1. Open http://localhost:5050 and log in.
2. Add a new server:
   - **Host**: `postgres` (the Compose service name)
   - **Port**: `5432`
   - **Database**: `casita`
   - **Username/Password**: `postgres` / `postgres`

## Build & test

```bash
dotnet build
dotnet test                          # when tests are added
```
