# Basket-Based Service Template Rules

This is the concrete rule set to follow when creating new services from the Basket service pattern.

## Canonical Structure

For each service create four runtime projects:

- `Ecommerce.<Service>Name.Service.Api`
- `Ecommerce.<Service>Name.Service.Application`
- `Ecommerce.<Service>Name.Service.Domain`
- `Ecommerce.<Service>Name.Service.Infrastructure`

Create matching test projects:

- `Ecommerce.<Service>Name.Service.Application.Tests`
- `Ecommerce.<Service>Name.Service.Domain.Tests`
- `Ecommerce.<Service>Name.Service.Infrastructure.Tests`
- add API tests when the service has real endpoint behavior worth protecting

The dependency direction must stay:

- `Domain -> no project references`
- `Application -> Domain`
- `Infrastructure -> Application + Domain`
- `Api -> Application + Infrastructure + ServiceDefaults`

## Coding and Architecture Rules

- Follow `.agents/shared/coding-standards.md`
- Follow `.agents/shared/tech-stack.md`
- Follow `.agents/shared/event-schemas.md`
- Shared code can only go to `src/SharedKernel`
- No shared database
- No MediatR
- No repository abstraction layer on top of EF Core
- Application handlers should use the service `DbContext` abstraction directly
- Domain should contain invariants, state transitions, domain events, and service-specific exceptions
- API should stay thin: binding, validation, handler call, result shape

## API Rules

- Use ASP.NET Core Minimal APIs
- Use versioned routes: `/api/v{version:apiVersion}/...`
- Add a root health/info endpoint returning `{ service = "<service-name>", status = "ok" }`
- Register `AddProblemDetails()`
- Register `AddExceptionHandler<GlobalExceptionHandler>()`
- Keep exception-to-problem-details mapping centralized
- Use FluentValidation for request validation
- Prefer shared endpoint filters/helpers to avoid duplicated validation plumbing
- Add OpenAPI and Scalar

## Application Rules

- One handler per use case
- Handlers expose `ExecuteAsync(...)`
- Command/query records live near the handler
- DTOs stay in Application
- `DependencyInjection/ServiceCollectionExtensions.cs` must register all handlers
- Query helpers may stay in `Queries/` when it improves readability

## Domain Rules

- Aggregate root per main workflow area
- Explicit status enum for state transitions
- Domain events are past tense and raised from aggregate methods
- Service-specific exception type should mirror the Basket shape:
  - error type
  - error code enum
  - validation/not-found/conflict factory methods
- Keep framework dependencies out of Domain

## Infrastructure Rules

- Use PostgreSQL + EF Core
- Each service owns its own `DbContext`
- Add `DbContextFactory` for migrations
- Keep EF configurations in `Persistence/Configurations`
- Keep interceptors in `Persistence/Interceptors`
- Add migrations into `Migrations/`
- Keep audit/soft-delete conventions aligned with Basket if the service needs them
- Use RabbitMQ + MassTransit
- Use an outbox for integration-event publishing
- Map domain events to versioned contracts in `Messaging/IntegrationEvents`
- Keep outbox publishing background service logic inside Infrastructure

## Solution and Hosting Rules

When a new service is added, update all of these:

- `Ecommerce.slnx`
- `src/Orchestration/Ecommerce.AppHost/AppHost.cs`
- `src/Gateway/Ecommerce.Gateway/appsettings.json`
- `src/Gateway/Ecommerce.Gateway/appsettings.Compose.json`
- `src/Gateway/Ecommerce.Gateway/Program.cs` if the service OpenAPI should be exposed in Scalar
- Dockerfiles and launch settings for the new API

## Naming Rules

- Service runtime host names should be kebab-case, for example:
  - `basket-api`
  - `order-api`
  - `logistics-api`
- Database connection names should match AppHost registration, for example:
  - `BasketDb`
  - `OrderDb`
  - `LogisticsDb`
- Integration event type names should be explicit and versioned

## Minimum Done Checklist

- Four service projects created
- Test projects created and added to solution
- AppHost wires database, broker, and service project
- Gateway routes to the new API
- Service root endpoint works
- Service OpenAPI document loads
- Initial migration exists
- Domain events mapped to integration contracts
- Outbox publishing is wired
- Problem details and validation behavior are consistent with Basket
