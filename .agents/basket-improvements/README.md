# Basket Improvement Tasks

This folder splits the Basket improvement plan into smaller phase-based tasks so multiple agents can work in parallel where safe.

## Recommended Order

1. `01-baseline-and-safety.md`
2. `02-outbox-pattern.md`
3. `03-api-contract-cleanup.md`
4. `04-domain-hardening.md`
5. `05-deletion-model-cleanup.md`
6. `06-startup-and-migrations.md`
7. `07-testing.md`

## Parallelism Notes

- `01-baseline-and-safety.md` can run first to document current behavior and identify immediate risks.
- `02-outbox-pattern.md` should be treated as a dependency for later integration tests.
- `03-api-contract-cleanup.md` and `04-domain-hardening.md` can overlap only if agents coordinate carefully on shared Basket files.
- `05-deletion-model-cleanup.md` depends on domain direction from phase 4.
- `06-startup-and-migrations.md` is mostly independent and can run in parallel with later stages.
- `07-testing.md` should run after the core implementation phases stabilize.

## Shared Scope

Only modify Basket service files unless a narrowly scoped shared change is required.
