# Ecommerce Agent Plan

## Goal

Build a distributed `Ecommerce` solution with simple business logic and broad technology coverage.

## Services

- `Basket`
- `Order`
- `Logistics`
- `Gateway`
- `Contracts`
- `Common`
- `AppHost`
- `ServiceDefaults`

## Architecture

- Event-driven between services
- HTTP at the edge through Gateway
- Separate database per service
- No shared database
- No synchronous service-to-service checkout flow

## Canonical Flow

`Gateway -> Basket -> BasketCheckedOut -> Order -> OrderCreated -> Logistics -> ShipmentReserved/ShipmentFailed -> OrderConfirmed/OrderCancelled`

## Naming

- Solution: `Ecommerce.sln`
- Projects: `Ecommerce.*`

## Rule

Keep business workflows minimal. Keep technology coverage visible.
