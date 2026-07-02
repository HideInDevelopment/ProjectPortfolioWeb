# PortfolioWeb Plan

## Goal
Build a solid backend for PortfolioWeb that can:

- serve as a real MVP
- be deployed with reasonable security
- be consumed later by a frontend
- evolve afterwards into a multi-user model

## Current status

### Completed
- [x] Base solution structure split into separate projects:
  - `Domain`
  - `Core.Contracts`
  - `Application.Contract`
  - `Application`
  - `Infrastructure`
  - `Api`
  - test projects by layer
- [x] Initial domain model with `Author` and `Project`
- [x] `Author 1:N Project` relationship
- [x] Persistence with EF Core + PostgreSQL
- [x] Entity configuration and initial migration
- [x] Basic CRUD for `Author` and `Project`
- [x] DTOs, manual mappers, and services
- [x] CRUD endpoints in the API
- [x] OpenAPI + Scalar
- [x] Automatic migration application when the API starts outside the `Testing` environment
- [x] Dockerization of the API and PostgreSQL
- [x] Initial exception handling:
  - functional validations in services
  - application exceptions
  - infrastructure exceptions
  - centralized HTTP translation in `GlobalExceptionHandler`
- [x] Final API hardening:
  - explicit validation of input DTOs at the HTTP edge
  - homogeneous validation responses with `ValidationProblemDetails`
  - pre-persistence validation for lengths, email format, `Version`, and `ReleaseDate`
  - more stable error contracts for frontend consumption
- [x] Structured logging in `Application` and `Api`
- [x] Initial automated suite:
  - `Application` tests
  - `Infrastructure` tests on the happy path
  - `Api` tests
- [x] Sequential test execution script
- [x] Optional `pre-push` hook prepared to validate tests before pushing changes
- [x] Technical slice `User + Authentication + Authorization` completed:
  - registration and login with local credentials
  - password hashing
  - JWT bearer
  - `User 1:1 Author` relationship
  - ownership over `Author` and `Project` write operations
  - active user revalidation on protected endpoints
  - unit, API, and integration tests around auth/authz
- [x] Testing slice largely completed:
  - unit tests in `Application`
  - integration tests in `Infrastructure`, including real PostgreSQL coverage for critical paths
  - API tests for contracts, validation, auth/authz, and visible error handling
  - one full authenticated happy path under real HTTP
  - destructive review of the suite performed
  - testing strategy documented in ADR

## Still pending before the MVP can be considered closed

### 1. Baseline security
- [x] Define and apply the minimum required security for a deployable backend:
  - secrets moved out of tracked Docker configuration into local `.env`
  - startup validation for required auth and database configuration
  - controlled error surface for infrastructure and unexpected failures
  - OpenAPI and Scalar disabled by default outside local development/testing unless explicitly enabled

### 2. Technical slice `User + Authentication + Authorization`
- [x] Completed

#### Delivered scope
- [x] `User` entity with `1:1` relationship to `Author`
- [x] Registration and login with `Email + PasswordHash`
- [x] JWT bearer for authentication
- [x] Combined `User + Author` creation
- [x] Ownership over `Author` and `Project` write operations
- [x] `IsActive` revalidation on protected endpoints
- [x] Duplicate email translation both in pre-check and in real persistence
- [x] Unit, API, and integration tests around auth/authz

#### Conscious decisions kept
- [x] No refresh tokens in this phase
- [x] No `Me` endpoint
- [x] No complex roles
- [x] Simple `Role` with initial value `User`

### 3. Exit criteria for deployment
- [ ] Define a minimum reproducible deployment flow
- [ ] Verify that the API container works with clean external configuration
- [ ] Leave the migration strategy resolved for deployed environments
- [ ] Add basic remote automation
  - CI for build + test at minimum

## Recommended execution order
1. Apply baseline security linked to the authentication/authorization already implemented
2. Close deployment criteria

## Out of scope for now
- Frontend
- `Me` endpoint
- refresh tokens
- advanced roles
- advanced multi-user features beyond the auth/authz baseline
- features not needed to expose portfolio and projects

## Practical definition of a closed MVP
We will consider the MVP closed when all of the following are true:

- stable CRUD for `Author` and `Project`
- `User 1:1 Author` model implemented
- working registration and login with JWT
- minimum authorization to protect owned authors and projects
- main validations resolved before persistence
- automated tests covering happy path and relevant negative cases
- reproducible deployment with reasonable external configuration
