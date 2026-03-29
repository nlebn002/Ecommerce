# Phase 3: API Contract Cleanup

## Goal

Make Basket API semantics explicit and predictable by separating reads from writes.

## Scope

- Basket API
- Basket application handlers
- related tests

## Tasks

1. Stop creating baskets from `GetBasketHandler`.
2. Add an explicit basket creation endpoint and use case:
   - `POST /baskets`
   - request contains `CustomerId`
3. Keep `GET /baskets/{basketId}` strictly read-only.
4. Return `404` when a basket is missing.
5. Update endpoint mappings, validators, exceptions, and tests.

## Acceptance Criteria

- `GET` has no write side effects.
- Basket creation is explicit.
- API behavior is covered by tests.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Api/EndpointMappings/*`
- `src/Services/Basket/Ecommerce.BasketService.Api/Endpoints/*`
- `src/Services/Basket/Ecommerce.BasketService.Application/Handlers/*`
- `src/Services/Basket/Ecommerce.BasketService.Application/Dtos/*`
- Basket API/application tests

## Constraints

- Coordinate with domain-hardening work if both agents touch the same handlers.
