# ADR-0001: Testing Strategy

## Status
Accepted

## Context
PortfolioWeb is a small ASP.NET Core API intended to be solid enough for deployment and clear enough to present as portfolio work.

The solution already contains tests at multiple levels:

- unit tests for application logic
- integration tests for infrastructure persistence behavior
- API tests for HTTP contracts, authentication, authorization, and visible error handling

At the same time, not every technical detail provides the same value when tested. Some code is core behavior. Some code is only plumbing or framework glue. Chasing coverage for its own sake would add noise, raise maintenance cost, and make the test suite harder to trust.

We need an explicit rule for what we test, what we do not test, and why.

## Decision
We use a layered testing strategy with a pragmatic bias toward real behavior over raw coverage percentage.

### Unit tests
Unit tests are the default choice for isolated logic with meaningful business or orchestration value, including:

- application services
- manual mappers
- security helpers with deterministic logic
- exception classifiers or similar isolated utility logic

These tests should be fast, focused, and clear about the rule they protect.

### Integration and API tests
Integration or API tests are preferred when the behavior depends on framework wiring or real infrastructure, including:

- EF Core persistence behavior
- PostgreSQL interaction
- HTTP validation and response contracts
- JWT authentication and authorization
- global exception handling
- end-to-end authenticated happy paths

These tests protect the parts most likely to fail only when the application is assembled for real.

### What we intentionally do not chase
We do not pursue coverage in plumbing, startup glue, or framework delegation code unless breaking that code would realistically break the application in a way not already protected elsewhere.

In practice, this means:

- we do not add tests only to increase the coverage number
- we stop adding low-value tests when the real behavior is already covered at a more meaningful level
- we accept that some boilerplate or passive configuration may remain indirectly tested rather than directly tested

### When to prefer a broader system-level test
A broader HTTP or system-style test is preferred over extra lower-level tests when:

- the value lies in proving the full slice works together
- authentication or authorization is part of the scenario
- repository, service, and controller wiring is part of the risk
- a realistic user flow is more important than isolated internal detail

For the current MVP, one real authenticated happy path is enough. A larger BDD or system-test layer is not introduced unless the application grows in complexity enough to justify it.

## Consequences
This strategy gives the project:

- a test suite centered on reliability instead of vanity coverage
- clearer criteria for where new tests should be added
- lower maintenance cost than testing every layer mechanically
- enough breadth to demonstrate good engineering judgment in a portfolio context

It also means some low-level glue may remain untested directly. That is acceptable as long as the real user-visible behavior is already protected by stronger tests.
