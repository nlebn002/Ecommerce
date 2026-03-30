# Order Service Creation Guide

Use Basket as the structural template, but adapt the behavior to the order workflow defined in `.agents/shared/event-schemas.md`.

## Goal

Create an `Order` service that consumes basket checkout events and manages order state until confirmation or cancellation.

Canonical flow:

- consume `BasketCheckedOut`
- create an order
- publish `OrderCreated`
- react to logistics outcome
- publish `OrderConfirmed` or `OrderCancelled`

## Projects To Create

Under `src/Services/Order/` create:

- `Ecommerce.OrderService.Api`
- `Ecommerce.OrderService.Application`
- `Ecommerce.OrderService.Domain`
- `Ecommerce.OrderService.Infrastructure`

Under `tests/Services/Order/` create:

- `Ecommerce.OrderService.Application.Tests`
- `Ecommerce.OrderService.Domain.Tests`
- `Ecommerce.OrderService.Infrastructure.Tests`
- `Ecommerce.OrderService.Api.Tests` if API endpoints are more than trivial

## Recommended Domain Shape

Aggregate:

- `Order`

Core state:

- `Id`
- `CustomerId`
- `Status`
- `ItemsTotal`
- `ShippingPrice`
- `FinalTotal`
- timestamps and concurrency fields inherited from the base entity pattern

Suggested statuses:

- `Pending`
- `Confirmed`
- `Cancelled`

Suggested domain events:

- `OrderCreatedDomainEvent`
- `OrderConfirmedDomainEvent`
- `OrderCancelledDomainEvent`

Suggested exception codes:

- `OrderNotFound`
- `OrderInactive`
- `ShipmentAlreadyProcessed`
- `InvalidOrderState`
- `ConcurrencyConflict`

## Application Layer Work

Create handlers for the minimum workflow:

- `GetOrderHandler`
- `CreateOrderFromBasketCheckoutHandler`
- `ConfirmOrderHandler`
- `CancelOrderHandler`

Add DTOs for:

- order summary
- order details
- line items

Add a `IOrderDbContext` abstraction matching the Basket pattern.

## API Layer Work

Keep the API small at first. Recommended endpoints:

- `GET /api/v{version}/orders/{orderId}`
- `GET /api/v{version}/orders/by-customer/{customerId}` or query-based lookup if needed
- optional development/seed endpoint only if you need parity with Basket dev flows

Rules:

- versioned Minimal API endpoints
- centralized exception mapping
- FluentValidation
- thin handlers only
- service root endpoint returns `order-api`

## Infrastructure Layer Work

Persistence:

- create `OrderDbContext`
- add EF configurations
- create initial migration
- add design-time `OrderDbContextFactory`

Messaging:

- consume `Ecommerce.Contracts.V1.BasketCheckedOut`
- optionally also support `V2` if the upstream Basket publishes both versions
- publish:
  - `OrderCreated`
  - `OrderConfirmed`
  - `OrderCancelled`
- use outbox pattern identical to Basket

MassTransit guidance:

- consumers belong in Infrastructure
- incoming integration event handling should call application handlers, not domain logic directly from the consumer
- do not publish directly from domain code

## Contracts Work

Confirm or add these shared contracts under `src/SharedKernel/Ecommerce.Contracts`:

- `OrderCreated`
- `OrderConfirmed`
- `OrderCancelled`

Each contract must inherit from `IntegrationEvent` and include the standard metadata fields from the shared base type.

## Hosting and Routing Work

Update:

- `Ecommerce.sln`
- `src/Orchestration/Ecommerce.AppHost/AppHost.cs`
- `src/Gateway/Ecommerce.Gateway/appsettings.json`
- `src/Gateway/Ecommerce.Gateway/appsettings.Compose.json`
- `src/Gateway/Ecommerce.Gateway/Program.cs`

Expected AppHost additions:

- `OrderDb`
- `order-api`
- references from `order-api` to its database and `MessageBroker`
- gateway reference to `order-api`

## Test Expectations

Add at least:

- domain tests for order status transitions
- application tests for create/confirm/cancel flows
- infrastructure tests for outbox/integration-event mapping
- API tests for problem details and successful reads

## Practical Implementation Order

1. Create the four runtime projects and add them to the solution.
2. Create the test projects and add them to the solution.
3. Add the domain aggregate, statuses, events, and exception type.
4. Add the application handlers and DTOs.
5. Add `OrderDbContext`, EF mappings, migrations, and outbox plumbing.
6. Add MassTransit consumers and integration-event publishers.
7. Add the API endpoints and exception handling.
8. Wire AppHost and Gateway.
9. Add tests.

## Keep Aligned With Basket

- Same project naming style
- Same startup style in `Program.cs`
- Same problem-details behavior
- Same outbox structure
- Same test project placement
- Same ServiceDefaults integration
