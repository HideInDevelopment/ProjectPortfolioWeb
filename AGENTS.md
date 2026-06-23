# PortfolioWeb Agent Guide

## What this repo is
- ASP.NET Core API on **.NET 10**
- Simple modular monolith with these projects:
  - `PortfolioWeb.Domain`: entities only
  - `PortfolioWeb.Core.Contracts`: repository contracts + shared infrastructure-facing exceptions
  - `PortfolioWeb.Application.Contract`: DTOs, service contracts, application exceptions
  - `PortfolioWeb.Application`: services, mappers, DI, logging extensions
  - `PortfolioWeb.Infrastructure`: EF Core, PostgreSQL, repositories, migrations, EF exception classification
  - `PortfolioWeb.Api`: controllers, global exception handler, OpenAPI, Scalar
  - `*.Tests`: NUnit test projects per layer

## Current stack
- SDK pinned in [global.json](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/global.json)
- NUnit 4 + Moq
- EF Core 10 + Npgsql
- Scalar for API exploration

## Solution root
- Work from the solution root: `PortfolioWeb/`
- Solution file: [PortfolioWeb.sln](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/PortfolioWeb.sln)
- API entrypoint: [Program.cs](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/PortfolioWeb.Api/Program.cs)

## Commands
```powershell
# build
dotnet build PortfolioWeb.sln

# run API locally
dotnet run --project PortfolioWeb.Api

# run all tests sequentially
.\scripts\test-all.ps1

# faster rerun
.\scripts\test-all.ps1 -NoRestore

# run one test project
dotnet test .\PortfolioWeb.Application.Tests\PortfolioWeb.Application.Tests.csproj --no-restore

# docker: postgres + api
docker compose up --build

# add EF migration
dotnet ef migrations add <Name> --project PortfolioWeb.Infrastructure --startup-project PortfolioWeb.Api

# apply EF migration
dotnet ef database update --project PortfolioWeb.Infrastructure --startup-project PortfolioWeb.Api
```

## Runtime behavior that matters
- API exposes OpenAPI and Scalar:
  - `/openapi/v1.json`
  - `/scalar`
- On startup, the API auto-applies pending EF migrations in every environment **except** `Testing`.
- Docker runs the API in `Development`.
- PostgreSQL runs in Docker via [docker-compose.yml](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/docker-compose.yml).

## Config files
- Real app settings live under [PortfolioWeb.Api](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/PortfolioWeb.Api).
- `appsettings.json` is gitignored.
- `appsettings.Development.json` is also intended to be gitignored, but note the ignore entry currently uses lowercase `development`. On Windows that still works; on case-sensitive environments it may not.
- Placeholder file exists: [appsettings.placeholder.json](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/PortfolioWeb.Api/appsettings.placeholder.json)

## Tests
- Test framework is **NUnit**, not xUnit.
- Shared test runner script: [scripts/test-all.ps1](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/scripts/test-all.ps1)
- Any iteration that introduces code or changes existing code must be covered by tests.
- After adding or updating those tests, run the skill [skills/post-iteration-validation/SKILL.md](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/skills/post-iteration-validation/SKILL.md) to validate the appropriate scope.
- Coverage runs must use [coverage.runsettings](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/coverage.runsettings) so the exclusion policy is stable and explicit.
- Test order in that script:
  1. `PortfolioWeb.Application.Tests`
  2. `PortfolioWeb.Core.Contracts.Tests`
  3. `PortfolioWeb.Infrastructure.Tests`
  4. `PortfolioWeb.Api.Tests`
- Infrastructure tests use EF Core InMemory through `InMemoryDbContextFactory`.
- API integration tests use `TestWebApplicationFactory` and force environment `Testing`.
- The API test factory also clears logging providers to avoid Windows Event Log permission failures during test runs.

## Error handling
- HTTP error mapping is done in [GlobalExceptionHandler.cs](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/PortfolioWeb.Api/ExceptionHandling/GlobalExceptionHandler.cs).
- `ExceptionClassifier` in Infrastructure is **not** the HTTP mapper; it only classifies low-level EF / database exceptions before repositories translate them.

## Coding conventions already in use
- Controllers are thin and delegate to services.
- Services perform validation and orchestration.
- Repositories talk directly to `DbContext`; this repo intentionally does **not** add extra repository-pattern abstractions on top of that.
- Mapping is manual, no AutoMapper.
- Primary constructors are used in several classes.

## Git hooks
- Optional helper script: [scripts/install-git-hooks.ps1](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/scripts/install-git-hooks.ps1)
- It installs `scripts/git-hooks/pre-push.sample` as `.git/hooks/pre-push`
- The hook runs `.\scripts\test-all.ps1 -NoRestore`

## Practical advice for agents
- Read the touched slice end to end before editing: controller -> service -> repository -> tests.
- If you change a contract in `*.Contract` or `Core.Contracts`, expect fallout in tests and in the corresponding implementation project.
- Prefer updating existing tests over adding new layers of helpers.
- Do not close a code iteration without test coverage for the introduced behavior and a test run aligned with that scope.
- Keep changes small. This repo currently favors straightforward code over reusable abstractions.
