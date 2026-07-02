---
name: post-iteration-validation
description: Validate a recent coding iteration in this PortfolioWeb repository. Use when code was added or changed and the agent needs to decide the right test scope, optionally inspect code coverage for the touched slice, add missing tests when the benefit is clear, and then rerun validation before handoff.
---

# Post Iteration Validation

Use the smallest useful validation scope first.

## Decide whether this skill applies

- If only documentation changed, do not run this skill unless the user asks for validation anyway.
- If code was added or modified, use this skill.

## Validation flow

1. Identify the touched slice.
2. Run the smallest useful test scope for that slice.
3. Decide whether a coverage check is worth doing.
4. If coverage is worth checking, inspect it for the touched test project or slice.
5. If the coverage result shows clear missing tests with good effort/benefit, add those tests.
6. Rerun the relevant tests.
7. If coverage was part of the flow, rerun coverage after the test updates.
8. Report exactly what was run and what remains uncertain.

## Decide test scope

- If only one project or slice changed, run that test project first.
- If contracts changed, run at least:
  - `PortfolioWeb.Application.Tests`
  - the directly affected downstream test project
- If API behavior changed, run `PortfolioWeb.Api.Tests`.
- If repository or EF code changed, run `PortfolioWeb.Infrastructure.Tests`.
- If the change is broad or the user asks for full validation, run:
  - `.\scripts\test-all.ps1 -NoRestore`

## Preferred test commands

Run from the solution root.

### Single test project

```powershell
dotnet test .\PortfolioWeb.Application.Tests\PortfolioWeb.Application.Tests.csproj --no-restore --verbosity minimal --disable-build-servers -m:1
```

Swap the project path as needed:

- `.\PortfolioWeb.Infrastructure.Tests\PortfolioWeb.Infrastructure.Tests.csproj`
- `.\PortfolioWeb.Api.Tests\PortfolioWeb.Api.Tests.csproj`

### Full suite

```powershell
.\scripts\test-all.ps1 -NoRestore
```

## Coverage is conditional, not automatic

Do a coverage pass when the iteration changed behavior in places where missing tests are informative:

- services
- repositories
- controllers
- mappers
- validators
- exception handling
- branching logic

Skip coverage when the change is obviously not worth it:

- documentation
- comments
- naming-only refactors
- trivial wiring with no real behavior

## Preferred coverage approach

Use the versioned repo configuration in [coverage.runsettings](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/coverage.runsettings). Keep the scope narrow first.

### Single test project with coverage

```powershell
dotnet test .\PortfolioWeb.Application.Tests\PortfolioWeb.Application.Tests.csproj --no-restore --verbosity minimal --disable-build-servers -m:1 --collect:"XPlat Code Coverage" --settings .\coverage.runsettings
```

Swap the project path as needed:

- `.\PortfolioWeb.Infrastructure.Tests\PortfolioWeb.Infrastructure.Tests.csproj`
- `.\PortfolioWeb.Api.Tests\PortfolioWeb.Api.Tests.csproj`

### Full project coverage pass

Run the three test projects one by one with the same coverage options rather than trying to invent a different flow.

### Current curated exclusions

The final coverage analysis must exclude these files because, today, their cost outweighs their value:

- EF migrations
- `Program.cs`
- `PortfolioWebDbContextFactory.cs`
- generated code under `obj`

Treat that list as curated.

Do not expand it on your own just because another file looks awkward to test.

If another exclusion candidate appears, stop and report:

- which file is in doubt
- why its testing cost may outweigh the benefit
- why excluding it would or would not be justified

Leave the exclusion list unchanged until the user decides.

## Effort / benefit rule

Coverage is a tool to find useful missing tests, not a reason to inflate the suite.

Add tests when all of this is true:

- the changed code has real behavior
- the missing path is clear
- the test is cheap to write and maintain
- the added test protects against regression

Do not auto-add tests when the value is doubtful or the target is awkward, noisy, or low-value, for example:

- framework glue with no meaningful branch behavior
- generated code
- code whose only uncovered lines are boilerplate with no business value

## If effort / benefit is unclear

Do not infer the answer.

Stop before adding those tests and report:

- where the doubt starts
- why the coverage gap may or may not be worth covering
- what tradeoff is blocking the decision

Leave that coverage gap unimplemented for the current iteration until the user decides.

## Reporting

- If tests pass, report which project or suite was run.
- If coverage was checked, report the scope and the conclusion.
- If tests were added because of coverage, say which behavior they now protect.
- If validation stayed narrow, do not claim full validation.
- If you stopped on an effort/benefit doubt, say so explicitly.

## Repo-specific notes

- Prefer `--disable-build-servers -m:1` on direct `dotnet test` runs in this repo to avoid sticky local runner issues.
- `PortfolioWeb.Api.Tests` uses a `Testing` environment and should not auto-apply migrations.
- Use [coverage.runsettings](/C:/Users/manue/Repositories/PortfolioWeb/PortfolioWeb/coverage.runsettings) for coverage runs so the exclusion policy stays consistent.
