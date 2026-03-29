# Phase 1 Notes: Basket Baseline And Safety

## Current Checkout Flow

1. API entrypoint
   - `POST /api/v{version}/baskets/{basketId}/checkout`
   - Implemented in [CheckoutBasketEndpoint.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Api/Endpoints/CheckoutBasketEndpoint.cs)
   - The endpoint validates that `basketId` is not empty, then calls `CheckoutBasketHandler`.
2. Application handler
   - Implemented in [CheckoutBasketHandler.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Application/Handlers/CheckoutBasket/CheckoutBasketHandler.cs)
   - Loads the basket via `IBasketDbContext.Baskets.Include(b => b.Items)` from [BasketQueries.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Application/Queries/BasketQueries.cs).
   - Fails with:
     - `BasketNotFoundException` if the basket does not exist
     - `BasketConflictException` if the basket is already checked out
     - `BasketValidationException` if no active items exist
   - Calls `basket.Checkout()` and then `_dbContext.SaveChangesAsync()`.
3. Domain aggregate
   - Implemented in [Basket.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Domain/Aggregates/Basket/Basket.cs)
   - `Checkout()`:
     - sets `Status = CheckedOut`
     - raises `BasketCheckedOutDomainEvent`
     - copies current active items and current basket total into the event payload
4. EF Core persistence
   - Implemented in [BasketDbContext.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Infrastructure/Persistence/BasketDbContext.cs)
   - `SaveChangesAsync()`:
     - collects domain events from tracked entities before saving
     - commits the database transaction via `base.SaveChangesAsync(...)`
     - dispatches the captured domain events after the commit
     - clears in-memory domain events only after successful dispatch
   - Basket/item loading is done eagerly with `Include(b => b.Items)`, so checkout runs against the aggregate plus current items in one query.
5. Domain event dispatch
   - Implemented in [DomainEventDispatcher.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Infrastructure/Messaging/DomainEventDispatcher.cs)
   - Dispatch is type-switched. Only `BasketCheckedOutDomainEvent` is supported right now.
6. MassTransit publication
   - `DomainEventDispatcher` publishes two integration messages for one checkout:
     - `Ecommerce.Contracts.V1.BasketCheckedOut`
     - `Ecommerce.Contracts.V2.BasketCheckedOut`
   - Both messages share one generated `CorrelationId`.
   - MassTransit/RabbitMQ wiring is registered in [ServiceCollectionExtensions.cs](/C:/Temp/dotnet/Ecommerce/src/Services/Basket/Ecommerce.BasketService.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs)

## Baseline Risks

- Publish-after-commit gap: the basket can be persisted as checked out even if RabbitMQ publish fails afterward. There is no outbox yet.
- Duplicate publication risk: if a caller retries after a publish failure, later phases will need idempotency or outbox handling to avoid duplicate downstream effects.
- Strict dispatcher mapping: unknown domain events throw `InvalidOperationException`, so adding new events without updating the dispatcher will fail writes.
- Aggregate invariants rely on handlers: `Basket.Checkout()` does not guard against double checkout or empty baskets by itself. Current safety depends on the application layer.
- No current integration coverage for the EF-save to MassTransit boundary.

## Guardrails Added In This Phase

- Added domain tests in [BasketCheckoutTests.cs](/C:/Temp/dotnet/Ecommerce/tests/Services/Basket/Ecommerce.BasketService.Domain.Tests/BasketCheckoutTests.cs) covering the current checkout state change and emitted event payload.
