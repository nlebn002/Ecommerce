# Basket Microservice Improvements

## Goal

Raise the Basket microservice from prototype quality to a production-ready service with better reliability, clearer API semantics, stronger domain rules, and test coverage.

## Scope

Apply changes only to the Basket service:

- `src/Services/Basket/Ecommerce.BasketService.Api`
- `src/Services/Basket/Ecommerce.BasketService.Application`
- `src/Services/Basket/Ecommerce.BasketService.Domain`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure`
- related Basket tests under `tests`

Do not modify other services unless a shared contract change is explicitly required.

## Main Problems To Solve

1. Integration events are published directly after `SaveChangesAsync`, so database state and message delivery are not atomic.
2. `GET /baskets/{basketId}` creates a basket when it does not exist.
3. Important invariants live in handlers and validators instead of the domain model.
4. Basket item deletion behavior is inconsistent with the soft-delete infrastructure.
5. Database migrations run automatically on app startup.
6. There is no visible Basket test suite covering domain rules, handlers, and integration behavior.

## Target Architecture

### Reliability

- Introduce an outbox pattern for integration events.
- Persist outgoing integration messages in the same database transaction as aggregate changes.
- Publish outbox messages asynchronously in a background process.
- Make outbox publishing idempotent and observable.

### API Semantics

- Separate basket creation from basket retrieval.
- `GET` must be read-only.
- Introduce an explicit create endpoint or command.

### Domain Integrity

- Move invariant checks into the aggregate where appropriate.
- Prevent invalid state transitions inside domain methods.
- Keep handlers thin and orchestration-focused.

### Persistence Consistency

- Choose one deletion model for basket items.
- If soft-delete is kept, model it explicitly in the aggregate and entity.
- If hard-delete is preferred, remove soft-delete assumptions that do not apply to basket items.

### Operations

- Run migrations manually or in a dedicated deployment step.
- Keep runtime startup simple and deterministic.

### Quality

- Add domain, application, and integration tests for Basket.

## Implementation Plan For Writer Agent

### Phase 1: Baseline And Safety

1. Inspect current Basket runtime flow:
   - request -> handler -> aggregate -> EF save -> event dispatch -> RabbitMQ
2. Document the current behavior in code comments only where needed.
3. Add or update tests around current checkout behavior before major refactoring if feasible.

### Phase 2: Outbox Pattern

1. Add an outbox entity and EF mapping in Basket infrastructure.
2. Convert domain events into persisted outbox records during `SaveChangesAsync`.
3. Remove direct message publication from the transaction path.
4. Add a background publisher service that:
   - reads pending outbox messages
   - publishes them via MassTransit
   - marks them processed
   - handles retry-safe behavior
5. Keep message contract mapping out of `DbContext`; place it in dedicated infrastructure components.
6. Update DI registration for the new background publisher and supporting services.

Acceptance criteria:

- Checkout persists basket state and outbox records atomically.
- A transient broker failure does not roll back the basket commit.
- Messages are publishable later from the outbox.

### Phase 3: API Contract Cleanup

1. Stop creating baskets from `GetBasketHandler`.
2. Add an explicit basket creation use case:
   - `POST /baskets`
   - input should contain `CustomerId`
3. Keep `GET /baskets/{basketId}` read-only.
4. Return `404` for missing baskets instead of creating them implicitly.
5. Update validators, exceptions, and endpoint mappings accordingly.

Acceptance criteria:

- `GET` has no write side effects.
- Basket creation is explicit and test-covered.

### Phase 4: Domain Hardening

1. Move key business rules into the aggregate:
   - cannot checkout an already checked-out basket
   - cannot checkout an empty basket
   - cannot add invalid quantities or prices
   - cannot mutate inactive baskets
2. Keep handler-level validation for transport concerns only.
3. Review `BasketItem` so it protects its own valid state.

Acceptance criteria:

- Invalid state changes are prevented even if handlers are bypassed.

### Phase 5: Deletion Model Cleanup

1. Decide whether basket items should be hard-deleted or soft-deleted.
2. If soft-delete:
   - add explicit domain method for item deletion
   - stop removing items only from the collection
   - verify EF query filters and aggregate behavior align
3. If hard-delete:
   - simplify model and remove soft-delete assumptions for basket items

Acceptance criteria:

- Deletion behavior is intentional, consistent, and reflected in tests.

### Phase 6: Startup And Migrations

1. Remove automatic database migration from runtime startup outside development, or remove it completely.
2. Keep design-time `DbContextFactory`.
3. Keep migration creation manual through the existing script.
4. Ensure local development instructions are clear.

Acceptance criteria:

- Service startup does not mutate schema unexpectedly in production-style runs.

### Phase 7: Testing

1. Add domain tests for:
   - add/update item
   - remove item
   - checkout invariants
   - total calculation
2. Add application tests for handlers.
3. Add integration tests for:
   - API read/write behavior
   - checkout creates outbox message
   - outbox publisher publishes and marks messages processed
4. Add failure-path tests where practical.

Acceptance criteria:

- Basket service has meaningful regression coverage around domain and infrastructure behavior.

## Suggested Work Order

1. Outbox infrastructure
2. API contract cleanup
3. Domain hardening
4. Deletion model cleanup
5. Startup migration cleanup
6. Tests and verification

## Files Likely To Change

- `src/Services/Basket/Ecommerce.BasketService.Api/Program.cs`
- `src/Services/Basket/Ecommerce.BasketService.Api/EndpointMappings/*`
- `src/Services/Basket/Ecommerce.BasketService.Api/Endpoints/*`
- `src/Services/Basket/Ecommerce.BasketService.Application/Handlers/*`
- `src/Services/Basket/Ecommerce.BasketService.Domain/Aggregates/Basket/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Persistence/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Messaging/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/DependencyInjection/*`
- `tests/*Basket*`

## Non-Goals

- Full redesign of other microservices
- Global platform refactor
- Premature abstraction of every shared pattern across the whole solution

## Definition Of Done

- Basket no longer publishes integration events directly from `SaveChangesAsync`
- Basket retrieval is read-only
- Basket creation is explicit
- Core invariants are enforced in the domain
- Deletion behavior is consistent
- Runtime startup does not auto-migrate schema unexpectedly
- Basket tests cover the critical paths
