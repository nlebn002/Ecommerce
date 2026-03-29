# Phase 6: Startup And Migrations

## Goal

Make Basket startup predictable and keep schema changes under explicit operational control.

## Scope

- Basket API startup
- Basket persistence tooling
- scripts/docs if needed

## Tasks

1. Remove automatic database migration from runtime startup outside development, or remove it entirely.
2. Keep the design-time `BasketDbContextFactory`.
3. Keep migration creation manual through the existing script.
4. Clarify local development workflow in code comments or docs if needed.

## Acceptance Criteria

- Service startup does not unexpectedly mutate schema in production-style runs.
- Manual migration workflow still works.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Api/Program.cs`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Persistence/BasketDbContextFactory.cs`
- `scripts/add-basket-migration.ps1`
- Basket docs if applicable

## Constraints

- Do not over-engineer deployment tooling in this phase.
