# Nutrify

A full-stack nutrition tracking application built with .NET 10, React 19, and .NET Aspire. Track food items, log daily intake, and view nutritional dashboards — all with per-user data isolation via Keycloak authentication.

## Tech Stack

### Backend
- **ASP.NET Core 10** — Minimal API endpoints
- **.NET Aspire 13.1.1** — Service orchestration, health checks, telemetry
- **Entity Framework Core 10** — Code-first with Fluent API configurations
- **PostgreSQL** — Primary database (with pgAdmin)
- **Redis** — Distributed caching
- **Keycloak** — OpenID Connect identity provider (JWT Bearer auth)
- **OpenTelemetry 1.15** — Distributed tracing, metrics, and logging

### Frontend
- **React 19** with TypeScript 5.7
- **Vite 6** — Build tooling
- **TanStack Router** — File-based routing
- **TanStack Query** — Server state management and caching
- **Tailwind CSS v4** — Utility-first styling
- **oidc-spa** — OpenID Connect integration

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/) (with npm)
- A container runtime compatible with .NET Aspire (Docker Desktop or Podman)

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd Nutrify
```

### 2. Restore dependencies

```bash
# .NET packages
dotnet restore

# Node.js packages (frontend)
cd src/Nutrify.Client
npm install
cd ../..
```

### 3. Run with Aspire

The AppHost project orchestrates everything — PostgreSQL, Redis, Keycloak, the API, and the React frontend are all started automatically:

```bash
dotnet run --project src/Nutrify.AppHost
```

This will:
- Start **PostgreSQL** with a `nutrify` database and **pgAdmin** for management
- Start **Redis** with **Redis Commander** for inspection
- Start **Keycloak** on port `8080`, importing the `nutrify` realm automatically
- Start the **API** (waits for database, Redis, and Keycloak to be healthy)
- Start the **React dev server** on port `5173` via Vite (waits for the API)
- Open the **Aspire Dashboard** in your browser for monitoring all services

### 4. Access the application

| Service | URL |
|---|---|
| Aspire Dashboard | Shown in terminal output on startup |
| React Frontend | http://localhost:5173 |
| API | https://localhost:5001 (proxied through Vite) |
| Keycloak Admin | http://localhost:8080 (admin/admin) |
| pgAdmin | Assigned by Aspire (see dashboard) |
| Redis Commander | Assigned by Aspire (see dashboard) |

### 5. Log in

A test user is pre-configured in the Keycloak realm:

| Field | Value |
|---|---|
| Username | `testuser` |
| Password | `testuser` |
| Email | `testuser@nutrify.local` |

## Architecture

### Backend

The API uses **minimal API endpoint groups** with scoped services and constructor-injected dependencies. All data is tenant-isolated by `UserId` extracted from the JWT claims.

**Error handling** is centralized via `GlobalExceptionHandler` middleware, which maps exceptions to RFC 7807 Problem Details:
- `ArgumentException` → 400 Bad Request
- `KeyNotFoundException` → 404 Not Found
- `InvalidOperationException` → 409 Conflict

**Database migrations** are applied automatically in development mode on startup.

### Frontend

The React app uses a **feature-based architecture** with shared components and utilities:

- **Routes** are file-based via TanStack Router (`/dashboard`, `/food-items`, `/intake`, `/categories`)
- **Server state** is managed by TanStack Query with 30s stale time and automatic refetch on window focus
- **Authentication** is handled by `oidc-spa` connecting to Keycloak via PKCE
- **API calls** go through a shared `apiClient` that automatically attaches Bearer tokens
- The Vite dev server **proxies** `/api` requests to the backend, with Aspire service discovery support

### Aspire Orchestration

The AppHost wires infrastructure and application services with dependency ordering:

```
PostgreSQL (+ pgAdmin)
Redis (+ Redis Commander)      →  API  →  React Client
Keycloak (realm auto-import)  ↗        ↗
```

All services use Aspire health checks, OpenTelemetry, and service discovery.

## Development

### Running individual projects

If you need to run specific parts independently:

```bash
# API only (requires PostgreSQL, Redis, and Keycloak running separately)
dotnet run --project src/Nutrify.Api

# Frontend only (requires API running)
cd src/Nutrify.Client
npm run dev
```

### Running tests

```bash
dotnet test
```

### Adding EF Core migrations

```bash
dotnet ef migrations add <MigrationName> --project src/Nutrify.Api
```

### Linting the frontend

```bash
cd src/Nutrify.Client
npm run lint
```

### Building the frontend for production

```bash
cd src/Nutrify.Client
npm run build
```

## Package Management

This project uses [NuGet Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management). All package versions are defined in [Directory.Packages.props](Directory.Packages.props) at the solution root. Individual `.csproj` files reference packages without specifying versions.
