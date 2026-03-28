# Aspire Orchestrator - Context

## Purpose

`.NET Aspire` is used for local orchestration of the distributed `Ecommerce` solution.

It is responsible for:

- starting infrastructure dependencies
- wiring service discovery and configuration
- applying shared telemetry defaults
- giving a single local entrypoint for the system

## Projects

```text
src/Orchestration/Ecommerce.AppHost/
src/Orchestration/Ecommerce.ServiceDefaults/
src/Services/Basket/Ecommerce.Basket.Api/
src/Services/Order/Ecommerce.Order.Api/
src/Services/Logistics/Ecommerce.Logistics.Api/
src/Gateway/Ecommerce.Gateway/
```

## Infrastructure

- PostgreSQL per service
- RabbitMQ for messaging
- Redis where required

## Responsibilities

### AppHost

- defines projects and containers
- sets references between services and infrastructure
- exposes the dashboard and local endpoints

### ServiceDefaults

- configures OpenTelemetry
- configures health checks
- configures resilience and shared host defaults

## Flow

```text
Client -> Gateway -> Basket API
Basket API -> RabbitMQ -> Order API
Order API -> RabbitMQ -> Logistics API
Logistics API -> RabbitMQ -> Order API
```

## Done When

- AppHost starts the full system locally
- Services receive the right connection settings
- Health and telemetry are enabled consistently
