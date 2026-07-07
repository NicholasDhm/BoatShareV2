# CLAUDE.md

**BoatShare v2** — quota-based boat reservation platform. Angular 18 (`client/`) + .NET 9 Web API (`server/boat-share/`), EF Core 9 + PostgreSQL (Npgsql; Sqlite/InMemory for tests). Railway (API, via Dockerfile) + Vercel (client).

> **The README has stale sections** — it mentions DynamoDB, ".NET 8", and a `client copy/` folder. None of that is current. Trust this file and the code: datastore is EF Core + PostgreSQL, the client folder is `client/`.

## Commands

```bash
cd client && npm start                    # Angular dev server :4200
cd client && npm test                     # Jasmine/Karma
cd client && npm run build

cd server/boat-share && dotnet run        # API — https :7122 / http :5041 (launchSettings)
cd server/boat-shareTests && dotnet test  # integration tests
```

Swagger UI: `https://localhost:7122/swagger`

## Structure

- `client/src` — `app/`, `components/` (`ui-calendar`, `ui-card`, `ui-navigation`), `pages/` (dashboard, login, profile), `services/`, `models/`, `auth/`
- `server/boat-share` — `Controllers → Services → Data` (EF Core DbContext); plus `DTOs/`, `Models/`, `Abstract/` (interfaces), `Middleware/`, `Converters/`, `Migrations/`
- EF migrations: `dotnet ef migrations add <Name>` in `server/boat-share` — never hand-edit an applied migration

## Domain

- Quota types: **Standard**, **Substitution**, **Contingency** — reservations are made against a user's quotas
- Reservation statuses: **Pending**, **Confirmed**, **Unconfirmed**
- JWT auth; admin role manages users and boats
