# Tech Stack

## Runtime

- .NET 10
- C# 14

## API

- ASP.NET Core Minimal API
- API Versioning
- OpenAPI + Scalar
- Problem Details

## Messaging

- RabbitMQ
- MassTransit 8.x
- Versioned integration events
- Outbox / Inbox

## Persistence

- PostgreSQL
- EF Core 10
- Npgsql

## Caching

- Redis

## Gateway

- YARP
- Rate Limiting

## Orchestration

- .NET Aspire
- `Ecommerce.AppHost`
- `Ecommerce.ServiceDefaults`
- Docker Compose

## Observability

- OpenTelemetry
- Serilog
- Prometheus
- Grafana

## Health

- `/health/live`
- `/health/ready`

## Validation

- FluentValidation

## Testing

- xUnit
- FluentAssertions
- Bogus
- Testcontainers
- MassTransit Test Harness
- WebApplicationFactory
- NBomber

## DevOps

- Dockerfiles
- GitHub Actions
- Central Package Management

## Forbidden

- MediatR
- Shared database
- Synchronous inter-service checkout flow
- Don't use Result pattern
- Don't use Repository pattern. Use direct dbContext.