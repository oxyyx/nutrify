# Nutrify

Personal nutrition tracker: .NET 10 API + React SPA, orchestrated with .NET Aspire.
Single deployable: the SPA is built into the API's `wwwroot` and served by it, so
the whole app runs from one container/process.

## Development environment

- **Windows + PowerShell.** Shell examples that use POSIX syntax (e.g. inline
  `VAR=... command`) need the PowerShell form instead (`$env:VAR=...; command`).
- **Containers run on Podman (Podman Desktop), not Docker.** The `docker` CLI is not
  available; use `podman` for container/volume/image commands (e.g.
  `podman volume ls`, `podman volume rm`). Aspire detects Podman automatically.
- Local dev runs through the Aspire AppHost (`src/Nutrify.AppHost`), which starts
  PostgreSQL (+pgAdmin), Redis, Keycloak, the API, and the Vite dev server.
- **The running AppHost locks the API's `bin/Debug` output.** While it's running,
  `dotnet build`/`test`/`ef` targeting Debug fail with a file-in-use error. Either
  stop the AppHost or run those commands against Release (`-c Release`, and
  `dotnet ef ... --configuration Release`). Release uses a separate output folder.
- Keycloak dev realm is imported from `src/Nutrify.AppHost/Realms/nutrify-realm.json`
  **only on first start** — the Keycloak container uses a data volume, so realm JSON
  changes require deleting that volume (`podman volume ls | grep keycloak`) or making
  the same change in the dev admin console. Dev login: `testuser` / `testuser`.
  The SPA client's `defaultClientScopes` must include `basic` (Keycloak 25+ moved the
  `sub` claim there; without it the API returns "Token contains no subject claim").

## Layout

- `src/Nutrify.Api` — minimal API (endpoints → services → EF Core/Postgres).
  JWT auth via Keycloak; user isolation by `UserId` from the token's `sub` claim.
  Every query must be scoped to the caller's `UserId`.
- `src/Nutrify.Contracts` — request/response records shared by API and mirrored
  by hand in `src/Nutrify.Client/src/shared/lib/types.ts` (keep in sync!).
- `src/Nutrify.Client` — React 19 + Vite + TanStack Router/Query + Tailwind v4.
  File-based routes in `src/routes` (`routeTree.gen.ts` is generated — never edit).
  Adding/renaming a route file requires regenerating it (run the dev server or
  `npx vite build`) *before* `npm run build`/typecheck, or tsc errors on the
  unknown route. Feature folders under `src/features/*` with `api/`, `components/`,
  `hooks/`.
- `tests/Nutrify.Api.Tests` — **TUnit** (`[Test]`, `[Arguments]`, and its own
  `await Assert.That(...)` assertions), using the **EF Core InMemory** provider
  (see testing note below). Tests run in parallel, so each one builds its own
  uniquely-named context via `TestDb.Create()` — never share state between tests.
  For collection assertions where order is the thing under test, pass
  `CollectionOrdering.Matching`; plain `IsEquivalentTo` ignores order.

## Commands

- Build all: `dotnet build Nutrify.slnx`
- Tests: `dotnet test --project tests/Nutrify.Api.Tests` (add `-c Release` if the
  AppHost is running). TUnit runs on Microsoft.Testing.Platform, which `global.json`
  opts into (`"test": { "runner": "Microsoft.Testing.Platform" }`). That mode
  requires the `--project`/`--solution` flag — a bare path argument is rejected.
- Client (from `src/Nutrify.Client`): `npm run build` (tsc + vite), `npm run lint`
- EF migration (needs a dummy connection string because real config comes from Aspire).
  Migrations apply automatically on API startup.
  ```powershell
  # PowerShell (Windows dev). Add --configuration Release if the AppHost is running.
  $env:ConnectionStrings__nutrify = "Host=localhost;Database=nutrify;Username=x;Password=x"
  dotnet ef migrations add <Name> --project src/Nutrify.Api --output-dir Data/Migrations
  ```
  When a migration adds a **required** column to a table with existing rows, EF
  scaffolds a `defaultValue` that would fill history with zeros/blanks — hand-edit
  the migration to backfill real values (add nullable → `Sql(...)` UPDATE → alter
  to NOT NULL) so production data stays correct.

## Conventions

- UI is mobile-first: bottom tab navigation on small screens, sidebar on `md+`.
  New pages/components must work at 360px width; tables need a card alternative
  or horizontal scroll on mobile.
- Nutritional values are always per 100g/100mL; `Unit` is derived from `Type`
  (Food→g, Drink→mL), never set directly by clients.
- **Intake entries snapshot their food.** On creation, an `IntakeEntry` copies the
  food item's name, unit, and per-100 macros onto itself; `FoodItemId` is a nullable
  FK with `OnDelete(SetNull)`. This lets a food item be deleted without erasing
  history. Compute/display an entry's nutrition (and search it) from the snapshot on
  the entry, **not** the live `FoodItem`.
- **Runtime SPA config.** The client is a static bundle, so anything
  environment-specific (OIDC issuer, app version) is delivered at runtime via
  `GET /api/config`, never baked in at build time. Add new such values there and in
  `src/Nutrify.Client/src/config.ts`.
- **Keep queries provider-portable.** Service queries run against Postgres in prod
  but the InMemory provider in tests. Avoid Postgres-only translations (e.g. use
  `x.ToLower().Contains(term)` for case-insensitive search rather than
  `EF.Functions.ILike`, which InMemory can't translate).

## Deployment

- CI (`.github/workflows/publish-image.yaml`) has two jobs. `test` runs the TUnit
  suite; `publish-image` declares `needs: test`, so a red suite blocks the image
  entirely. `publish-image` is also guarded by `if: github.event_name == 'push'`,
  so pull requests run the tests as a check without publishing anything.
- The image build pushes to GHCR: push to `main` → `latest` + `sha-…`; a git tag
  `vX.Y.Z` → semver tags (`X.Y.Z`, `X.Y`, `X`) and refreshes `latest`.
- App version is computed from `git describe` in CI, passed as the `APP_VERSION`
  Docker build-arg → env var → `/api/config`, and shown next to the logo in the SPA.
  Locally (no build-arg) it reads `dev`.
- Deploy is pull-based: pushing to `main` triggers the image build; the server pulls
  the new image.
