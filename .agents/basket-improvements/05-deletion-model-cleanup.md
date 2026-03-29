# Phase 5: Deletion Model Cleanup

## Goal

Make basket item deletion behavior consistent with the chosen persistence strategy.

## Scope

- Basket domain
- Basket persistence
- related tests

## Tasks

1. Decide whether basket items should be soft-deleted or hard-deleted.
2. If soft-delete is kept:
   - add explicit domain behavior for item deletion
   - stop relying only on collection removal
   - verify EF query filters and interceptor behavior
3. If hard-delete is chosen:
   - simplify the aggregate and persistence model
   - remove soft-delete assumptions that do not apply
4. Add tests for the chosen deletion behavior.

## Acceptance Criteria

- Deletion behavior is intentional and consistent.
- Persistence and aggregate behavior match.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Domain/Aggregates/Basket/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Persistence/*`
- Basket tests

## Constraints

- Align with the domain-hardening direction before finalizing changes.
