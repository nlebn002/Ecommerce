# Technologies

## Platform

The solution targets `.NET 10.0` across application and test projects.

Primary platform choices:

- ASP.NET Core minimal APIs
- .NET Aspire for local orchestration
- EF Core with PostgreSQL
- MassTransit with RabbitMQ
- YARP reverse proxy
- OpenTelemetry for metrics, traces, and logs

## Application Stack

Core libraries visible in `Directory.Packages.props`:

- `Aspire.Hosting.AppHost`
- `Aspire.Hosting.PostgreSQL`
- `Aspire.Hosting.RabbitMQ`
- `Asp.Versioning.Http`
- `FluentValidation.DependencyInjectionExtensions`
- `MassTransit`
- `MassTransit.RabbitMQ`
- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Scalar.AspNetCore`
- `Yarp.ReverseProxy`

## Persistence

Persistence is built on:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Relational`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

Each business service has:

- its own `DbContext`
- its own EF Core migrations
- its own PostgreSQL database

The shared `Ecommerce.Common` project provides:

- entity base types
- auditing support through `AuditSaveChangesInterceptor`
- shared outbox abstractions

## Messaging

Asynchronous service-to-service communication uses:

- `MassTransit`
- `MassTransit.RabbitMQ`

The solution uses versioned integration contracts from `Ecommerce.Contracts` and combines them with an outbox publisher hosted service in each business service.

## API and Documentation

HTTP APIs use:

- ASP.NET Core minimal APIs
- API versioning via `Asp.Versioning.Http`
- OpenAPI generation via `Microsoft.AspNetCore.OpenApi`
- Scalar UI via `Scalar.AspNetCore`
- gateway aggregation via YARP

## Resilience and Discovery

`Ecommerce.ServiceDefaults` adds:

- service discovery
- standard HTTP resilience handlers
- default health checks

This is shared by the gateway and the three business APIs.

## Observability

Observability tooling includes:

- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.Runtime`
- `OpenTelemetry.Exporter.Prometheus.AspNetCore`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`

In Docker Compose, the external observability stack is:

- Prometheus
- Loki
- Promtail
- Grafana

## Testing

The test stack uses:

- `xunit`
- `FluentAssertions`
- `Microsoft.NET.Test.Sdk`
- `Microsoft.EntityFrameworkCore.InMemory` (will be updated with TestContainers)
- `Microsoft.AspNetCore.Mvc.Testing`

The repository includes domain, application, infrastructure, and API-oriented tests depending on the service.

## Tooling and Conventions

Repository-level conventions include:

- central package management through `Directory.Packages.props`
- shared build settings through `Directory.Build.props`
- PowerShell helper scripts for migration creation
- Docker-based local execution
- Aspire-based local orchestration
