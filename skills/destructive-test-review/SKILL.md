---
name: destructive-test-review
description: Review an existing test suite with an adversarial mindset after a coding slice is finished. Use when code and tests already exist and the goal is to detect false positives, happy-path bias, missing branches, weak assertions, mocked behavior that hides bugs, or tests that validate implementation details instead of user-visible behavior.
---

# Destructive Test Review

Assume the implementation may be wrong and the tests may still pass.

Your job is to break confidence, not to preserve it.

## When to use this skill

Use it after:

- the code is implemented
- the normal tests are already written
- the normal validation pass is already green

Do not use it as the first validation step.

## Goal

Find places where the current tests could be giving a false sense of safety.

## Inputs

Work from the smallest useful context:

- the touched slice
- the relevant files
- the tests that were added or updated
- the validation results already obtained

Prefer raw artifacts:

- diff
- file paths
- current tests
- current behavior

Do not rely on the previous agent's conclusions unless the task explicitly requires that context.

## Review mindset

Look for these failure modes:

- tests cover only happy path
- a branch exists but no test reaches it
- mocks are so strong that the test can never fail in a realistic way
- assertions are too weak
- assertions check that "something happened" but not the right behavior
- tests validate private implementation shape instead of observable behavior
- integration tests are missing where unit tests are not enough
- tests pass because the setup duplicates the implementation logic
- tests ignore ownership, authorization, invalid input, or persistence edge cases
- tests would still pass after a real regression

## High-value review targets in this repo

- services with branching logic
- repositories with persistence behavior
- exception-to-HTTP translation
- DTO validation at API boundary
- authorization and ownership rules
- any code path that depends on PostgreSQL behavior rather than just in-memory behavior

## Output

Produce findings, not praise.

For each finding, state:

1. where the weakness is
2. why the current test can miss a bug
3. what kind of test would catch it

If no serious weaknesses are found, say that explicitly, but still note residual risk.

## Boundaries

- Do not add or rewrite tests unless explicitly asked.
- Do not invent bugs without pointing to the exact gap that would allow them through.
- Focus on meaningful confidence gaps, not cosmetic coverage chasing.
- If the suite is already strong enough for the slice size and risk level, say so plainly.
