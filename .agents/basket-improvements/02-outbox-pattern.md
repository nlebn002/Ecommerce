# Phase 2: Outbox Pattern

## Goal

Make Basket integration event delivery reliable by replacing direct publish-on-save with an outbox pattern.

## Scope

- Basket infrastructure and related tests

## Tasks

1. Add an outbox entity and EF configuration.
2. Persist integration messages in the same transaction as aggregate changes.
3. Remove direct RabbitMQ publication from the `SaveChangesAsync` transaction path.
4. Add a background publisher service that:
   - reads pending outbox messages
   - publishes them through MassTransit
   - marks them processed
   - behaves safely on retry
5. Move contract mapping into dedicated infrastructure components.
6. Update DI registrations for outbox persistence and publishing.

## Acceptance Criteria

- Checkout commits basket changes and outbox records atomically.
- Broker failure does not lose the basket commit.
- Pending outbox messages can be published later.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Persistence/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Messaging/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/DependencyInjection/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/Migrations/*`
- Basket integration tests

## Constraints

- Keep changes Basket-local.
- Do not redesign other services.
