# syntax=docker/dockerfile:1
#
# Single-container image for Nutrify: the React client is built into static
# assets and served by the ASP.NET Core API out of wwwroot, so the whole app
# (UI + API) runs from one process.
#
# Build from the repository root (Podman shown; substitute `docker` if needed):
#   podman build -t nutrify .
#
# OIDC settings are supplied at runtime (not build time) via the API's
# configuration, e.g. -e Spa__OidcIssuer=... -e Spa__OidcClientId=...; by
# default the issuer is derived from the API's Keycloak settings.

# ---- Stage 1: build the React client ----
FROM node:22-slim AS client-build
WORKDIR /client

# Restore dependencies first for better layer caching.
COPY src/Nutrify.Client/package.json src/Nutrify.Client/package-lock.json ./
RUN npm ci

COPY src/Nutrify.Client/ ./
RUN npm run build

# ---- Stage 2: build & publish the API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

# Central build/package configuration required by every project's restore.
COPY Directory.Build.props Directory.Packages.props ./

# Copy project files first so 'dotnet restore' is cached independently of source.
COPY src/Nutrify.Api/Nutrify.Api.csproj src/Nutrify.Api/
COPY src/Nutrify.ServiceDefaults/Nutrify.ServiceDefaults.csproj src/Nutrify.ServiceDefaults/
COPY src/Nutrify.Contracts/Nutrify.Contracts.csproj src/Nutrify.Contracts/
RUN dotnet restore src/Nutrify.Api/Nutrify.Api.csproj

# Application source.
COPY src/Nutrify.Api/ src/Nutrify.Api/
COPY src/Nutrify.ServiceDefaults/ src/Nutrify.ServiceDefaults/
COPY src/Nutrify.Contracts/ src/Nutrify.Contracts/

# Drop the built SPA into wwwroot so it is published as static web assets.
COPY --from=client-build /client/dist/ src/Nutrify.Api/wwwroot/

RUN dotnet publish src/Nutrify.Api/Nutrify.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Stage 3: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Npgsql probes for Kerberos/GSSAPI support when connecting; without this
# library it logs a noisy (non-fatal) error on every connection attempt.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=api-build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Nutrify.Api.dll"]
