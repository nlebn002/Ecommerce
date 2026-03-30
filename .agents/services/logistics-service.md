# Logistics Service Creation Guide

Use Basket as the structural template and Order as the upstream business dependency.

## Goal

Create a `Logistics` service that reacts to order creation and decides whether shipping can be reserved, repriced, or failed.

Canonical flow:

- consume `OrderCreated`
- reserve shipment or fail shipment
- publish `ShipmentReserved` or `ShipmentFailed`
- optionally publish `ShipmentRepriced` when shipping cost changes

## Projects To Create

Under `src/Services/Logistics/` create:

- `Ecommerce.LogisticsService.Api`
- `Ecommerce.LogisticsService.Application`
- `Ecommerce.LogisticsService.Domain`
- `Ecommerce.LogisticsService.Infrastructure`

Under `tests/Services/Logistics/` create:

- `Ecommerce.LogisticsService.Application.Tests`
- `Ecommerce.LogisticsService.Domain.Tests`
- `Ecommerce.LogisticsService.Infrastructure.Tests`
- `Ecommerce.LogisticsService.Api.Tests` if the API contains meaningful behavior

## Recommended Domain Shape

Aggregate:

- `Shipment`

Core state:

- `Id`
- `OrderId`
- `Carrier`
- `ShippingPrice`
- `Status`

Suggested statuses:

- `Pending`
- `Reserved`
- `Failed`

Suggested domain events:

- `ShipmentReservedDomainEvent`
- `ShipmentFailedDomainEvent`
- `ShipmentRepricedDomainEvent`

Suggested exception codes:

- `ShipmentNotFound`
- `ShipmentAlreadyFinalized`
- `InvalidShipmentState`
- `InvalidShippingPrice`
- `ConcurrencyConflict`

## Application Layer Work

Create handlers for the minimum workflow:

- `GetShipmentHandler`
- `ReserveShipmentHandler`
- `FailShipmentHandler`
- `RepriceShipmentHandler`

Add a `ILogisticsDbContext` abstraction following the Basket pattern.

DTOs should expose:

- shipment summary
- shipment details
- reservation outcome

## API Layer Work

Keep the public API small and operational:

- `GET /api/v{version}/shipments/{shipmentId}`
- `GET /api/v{version}/shipments/by-order/{orderId}` if operations need lookup by order
- optional admin or dev endpoints only when needed

Rules:

- versioned Minimal APIs
- centralized problem details mapping
- FluentValidation
- service root endpoint returns `logistics-api`

## Infrastructure Layer Work

Persistence:

- create `LogisticsDbContext`
- add EF configurations
- add design-time factory
- create initial migration

Messaging:

- consume `OrderCreated`
- publish:
  - `ShipmentReserved`
  - `ShipmentRepriced`
  - `ShipmentFailed`
- use the same outbox pattern as Basket

Implementation guidance:

- Consumers should translate incoming contracts to application commands
- Domain methods decide state changes
- Integration-event factories map domain events to shared contracts

## Contracts Work

Confirm or add these shared contracts under `src/SharedKernel/Ecommerce.Contracts`:

- `ShipmentReservationRequested` if you decide to model the request explicitly
- `ShipmentReserved`
- `ShipmentRepriced`
- `ShipmentFailed`

Each contract must inherit from `IntegrationEvent`.

## Hosting and Routing Work

Update:

- `Ecommerce.sln`
- `src/Orchestration/Ecommerce.AppHost/AppHost.cs`
- `src/Gateway/Ecommerce.Gateway/appsettings.json`
- `src/Gateway/Ecommerce.Gateway/appsettings.Compose.json`
- `src/Gateway/Ecommerce.Gateway/Program.cs`

Expected AppHost additions:

- `LogisticsDb`
- `logistics-api`
- references from `logistics-api` to its database and `MessageBroker`
- gateway reference to `logistics-api`

## Test Expectations

Add at least:

- domain tests for shipment state transitions
- application tests for reserve/fail/reprice flows
- infrastructure tests for contract mapping and outbox publishing
- API tests for validation/problem details and successful reads

## Practical Implementation Order

1. Create the four runtime projects and add them to the solution.
2. Create the test projects and add them to the solution.
3. Create shipment aggregate, statuses, events, and exception type.
4. Add application handlers, DTOs, and db-context abstraction.
5. Add EF Core persistence, migrations, and outbox plumbing.
6. Add MassTransit consumers and outgoing integration events.
7. Add Minimal API endpoints and exception handling.
8. Wire AppHost and Gateway.
9. Add tests.

## Keep Aligned With Basket

- Same layering and naming style
- Same startup and service registration pattern
- Same exception-to-problem-details shape
- Same outbox implementation style
- Same orchestration/gateway integration pattern
