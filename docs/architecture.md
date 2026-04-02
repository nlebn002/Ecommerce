# Architecture

## Overview

This solution is a small event-driven e-commerce system built around independently deployable .NET services. The runtime shape is:

- `Ecommerce.Gateway`: the single HTTP entry point for clients.
- `Ecommerce.BasketService`: manages baskets and checkout initiation.
- `Ecommerce.OrderService`: creates and tracks orders.
- `Ecommerce.LogisticsService`: reserves shipments and determines shipping outcome.
- `Ecommerce.Contracts`: shared integration event contracts.
- `Ecommerce.Common`: shared infrastructure primitives such as entities, auditing, validation helpers, and outbox support.
- `Ecommerce.AppHost`: .NET Aspire orchestration for local development.
- `Ecommerce.ServiceDefaults`: shared service defaults for health checks, metrics, tracing, logging, resilience, and service discovery.

Each business service follows a layered structure:

- `Api`: minimal API endpoints, validation registration, OpenAPI, Scalar, and startup behavior.
- `Application`: commands, queries, handlers, and DTOs.
- `Domain`: aggregates, domain events, and business exceptions.
- `Infrastructure`: EF Core persistence, MassTransit consumers, outbox publishing, and dependency injection wiring.

## Runtime Topology

The system uses:

- PostgreSQL with one database per service:
  - `basketdb`
  - `orderdb`
  - `logisticsdb`
- RabbitMQ as the message broker for asynchronous integration events.
- YARP in the gateway for request routing.
- OpenTelemetry and Prometheus metrics in every HTTP process.

In Aspire, `Ecommerce.AppHost` provisions:

- PostgreSQL
- RabbitMQ
- basket API
- order API
- logistics API
- gateway

The gateway references the three APIs through service discovery. Each API references its own PostgreSQL database and RabbitMQ.

## Integration Workflow

The main business flow is asynchronous after basket checkout:

1. A client creates a basket and adds items through `BasketService`.
2. Basket checkout marks the basket as checked out and writes an outbox message.
3. Basket infrastructure publishes `BasketCheckedOut` to RabbitMQ.
4. `OrderService` consumes `BasketCheckedOut` and creates the order.
5. Order infrastructure publishes `OrderCreated`.
6. `LogisticsService` consumes `OrderCreated` and attempts shipment reservation.
7. Logistics publishes either:
   - `ShipmentRepriced` when shipment reservation succeeds and shipping cost is known, or
   - `ShipmentFailed` when reservation fails.
8. `OrderService` consumes the logistics event and either confirms or cancels the order.

This design keeps write operations local to a service boundary and uses integration events for cross-service coordination.

## Messaging and Reliability

All three business services use the outbox pattern:

- domain changes and outgoing integration messages are stored together in the service database
- a background hosted service polls the outbox table
- pending messages are published to RabbitMQ through MassTransit
- retries are managed through configurable outbox processor options

This reduces the risk of losing integration events when database writes succeed but message publication fails.

## Data Ownership

Each service owns its own schema and persistence model:

- `BasketService` owns basket aggregates and basket item rows.
- `OrderService` owns order aggregates and order item rows.
- `LogisticsService` owns shipment aggregates.

Cross-service reads are not performed through shared databases. Communication across boundaries happens through HTTP at the gateway edge and through integration events between services.

## Shared Contracts

`Ecommerce.Contracts` contains versioned integration event contracts:

- Basket checkout events in `V1` and `V2`
- Order events in `V1`
- Shipment events in `V1`

The order service currently consumes both `V1.BasketCheckedOut` and `V2.BasketCheckedOut`, which lets the system evolve the basket contract without immediately breaking downstream consumers.

## Observability and Operations

All HTTP services expose:

- `/health/live`
- `/health/ready`
- `/metrics`
- `/openapi/v1.json`
- `/scalar`

The full Docker Compose stack adds:

- Prometheus for metrics scraping
- Loki for log storage
- Promtail for Docker log shipping
- Grafana for dashboards and log exploration

## Tests

The solution includes test projects per service area, covering:

- domain logic
- application handlers
- infrastructure behavior
- API behavior for order and logistics

This keeps verification aligned with the service boundaries used in production.
