# Deployment

## Purpose
This document defines the minimum reproducible local deployment flow for the current PortfolioWeb backend MVP.

It is intentionally small. The goal is that someone can start the application locally, with external configuration only, and validate that it works step by step.

## Scope
Reference deployment path for this MVP:

- PostgreSQL in Docker
- API as a Docker container
- configuration provided externally through a local `.env` file
- automatic migration on startup for local evaluation

Out of scope for this document:

- Kubernetes
- managed cloud services
- CD pipelines
- secrets managers
- blue/green or zero-downtime deployment strategies

## Prerequisites
Fresh environment requirements:

- Docker
- .NET 10 SDK
- source checkout of this repository

## Required configuration
The application must not rely on tracked local config files.

Configuration is provided through a local `.env` file in the solution root.

Required values:

```text
POSTGRES_PASSWORD
AUTH_ISSUER
AUTH_AUDIENCE
AUTH_SIGNING_KEY
AUTH_EXPIRATION_MINUTES
```

Notes:

- the signing key must be at least 32 characters long
- for local evaluation, the provided Docker Compose flow keeps `Database__ApplyMigrationsOnStartup=true`
- OpenAPI and Scalar are available in this local flow because the API runs in `Development`

## Reference flow
This is the minimum local deployment flow supported by the repo today.

### 1. Create the local configuration file
From the solution root, create `.env` from the example file:

```powershell
Copy-Item .env.example .env
```

### 2. Edit `.env`
Open `.env` and set real values:

```text
POSTGRES_PASSWORD=change-me
AUTH_ISSUER=PortfolioWeb
AUTH_AUDIENCE=PortfolioWebClient
AUTH_SIGNING_KEY=ChangeThisSigningKeyToAStrongValueWithAtLeast32Chars
AUTH_EXPIRATION_MINUTES=60
```

### 3. Start the application
From the solution root, run:

```powershell
docker compose up --build
```

This starts:

- PostgreSQL
- the API on `http://localhost:8080`
- Scalar on `http://localhost:8080/scalar`

### 4. Wait for startup
The API applies pending migrations automatically in this local Docker flow.

Wait until the containers are healthy and the API is reachable.

## Verification checklist
Minimum manual verification after startup:

1. `GET http://localhost:8080/api/Authors` returns `200`
2. `POST /api/auth/register` creates a new user
3. `POST /api/auth/login` returns a JWT
4. `GET /api/auth/me` with that JWT returns the current user and author
5. `POST /api/Projects` with that JWT creates a project
6. `PUT /api/Projects/{id}` with that JWT updates the project
7. `GET /api/Projects/{id}` returns the updated project

Expected local behavior:

- the API starts without a local tracked `appsettings.json`
- the API reads its important secrets from `.env`
- the API container starts successfully
- the database is created or migrated automatically in this local Docker flow

### 5. Stop the application
To stop the containers:

```powershell
docker compose down
```

If you also want to remove the PostgreSQL volume:

```powershell
docker compose down -v
```

## Production-like note
The current local flow is intentionally optimized for easy evaluation.

For deployed environments, the intended policy is still:

- `Database__ApplyMigrationsOnStartup=false`
- migrations applied explicitly before normal API startup
- OpenAPI and Scalar disabled unless explicitly needed

That production-like path is a deployment policy note, but it is not the primary path for local evaluation.

## Current limitations
This local deployment flow is intentionally minimal and has some known simplifications:

- it is aimed at local evaluation, not production hosting
- it uses Docker Compose as the orchestration mechanism
- it does not include a health endpoint
- it does not include automated rollback or release orchestration

These are acceptable for the current backend MVP and can be upgraded later if the project grows beyond portfolio scope.
