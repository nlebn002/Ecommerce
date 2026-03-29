# Phase 4: Domain Hardening

## Goal

Move core business invariants into the Basket domain model so invalid state changes are blocked inside the aggregate.

## Scope

- Basket domain
- application handlers as needed
- related tests

## Tasks

1. Enforce domain rules in the aggregate:
   - cannot checkout an already checked-out basket
   - cannot checkout an empty basket
   - cannot add invalid quantities
   - cannot add invalid prices
   - cannot mutate inactive baskets
2. Keep handler validation focused on transport and request-shape concerns.
3. Harden `BasketItem` so it cannot enter invalid state.
4. Remove duplicated business rule logic from handlers where the aggregate now owns it.

## Acceptance Criteria

- Invalid state transitions fail even if handlers are bypassed.
- Domain tests cover the critical invariants.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Domain/Aggregates/Basket/*`
- `src/Services/Basket/Ecommerce.BasketService.Application/Handlers/*`
- Basket domain/application tests

## Constraints

- Avoid pushing infrastructure concerns into the domain.
