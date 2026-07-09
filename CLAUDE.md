# Nutrify

Personal nutrition tracker: .NET 10 API + React SPA, orchestrated with .NET Aspire.

## Development environment

- **Containers run on Podman (Podman Desktop), not Docker.** The `docker` CLI is not
  available; use `podman` for container/volume/image commands (e.g.
  `podman volume ls`, `podman volume rm`). Aspire detects Podman automatically.
- Local dev runs through the Aspire AppHost (`src/Nutrify.AppHost`), which starts
  PostgreSQL (+pgAdmin), Redis, Keycloak, the API, and the Vite dev server.
- Keycloak dev realm is imported from `src/Nutrify.AppHost/Realms/nutrify-realm.json`
  **only on first start** — the Keycloak container uses a data volume, so realm JSON
  changes require deleting that volume (`podman volume ls | grep keycloak`) or making
  the same change in the dev admin console. Dev login: `testuser` / `testuser`.
  The SPA client's `defaultClientScopes` must include `basic` (Keycloak 25+ moved the
  `sub` claim there; without it the API returns "Token contains no subject claim").

## Layout

- `src/Nutrify.Api` — minimal API (endpoints → services → EF Core/Postgres).
  JWT auth via Keycloak; user isolation by `UserId` from the token's `sub` claim.
- `src/Nutrify.Contracts` — request/response records shared by API and mirrored
  by hand in `src/Nutrify.Client/src/shared/lib/types.ts` (keep in sync!).
- `src/Nutrify.Client` — React 19 + Vite + TanStack Router/Query + Tailwind v4.
  File-based routes in `src/routes` (routeTree.gen.ts is generated — run
  `npx vite build` or the dev server to regenerate, never edit). Feature folders
  under `src/features/*` with `api/`, `components/`, `hooks/`.
- `tests/Nutrify.Api.Tests` — xUnit + FluentAssertions.

## Commands

- Build all: `dotnet build Nutrify.slnx`
- Tests: `dotnet test tests/Nutrify.Api.Tests`
- Client (from `src/Nutrify.Client`): `npm run build` (tsc + vite), `npm run lint`
- EF migration (needs dummy connection strings because config comes from Aspire):
  ```
  cd src/Nutrify.Api
  ConnectionStrings__nutrify="Host=localhost;Database=nutrify;Username=x;Password=x" \
    dotnet ef migrations add <Name> --output-dir Data/Migrations
  ```
  Migrations apply automatically on API startup.

## Conventions

- UI is mobile-first: bottom tab navigation on small screens, sidebar on `md+`.
  New pages/components must work at 360px width; tables need a card alternative
  or horizontal scroll on mobile.
- Nutritional values are always per 100g/100mL; `Unit` is derived from `Type`
  (Food→g, Drink→mL), never set directly by clients.