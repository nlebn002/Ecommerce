# Docker Compose

## Files

The repository contains two Docker Compose entry points:

- `docker-compose.yml`: full local stack with infrastructure, services, gateway, and observability tooling.
- `docker-compose-light.yml`: reduced stack with PostgreSQL, RabbitMQ, the three business APIs, and the gateway.

## Full Stack

`docker-compose.yml` starts these containers:

- `postgres`
- `rabbitmq`
- `basket-api`
- `order-api`
- `logistics-api`
- `gateway`
- `prometheus`
- `loki`
- `promtail`
- `grafana`

Published ports:

- `5010` -> gateway
- `5020` -> basket API
- `5030` -> order API
- `5040` -> logistics API
- `5432` -> PostgreSQL
- `5672` -> RabbitMQ AMQP
- `15672` -> RabbitMQ management UI
- `9090` -> Prometheus
- `3100` -> Loki
- `3000` -> Grafana

## Light Stack

`docker-compose-light.yml` is intended for a smaller local footprint. It starts:

- `postgres`
- `rabbitmq`
- `basket-api`
- `order-api`
- `logistics-api`
- `gateway`

It omits Prometheus, Loki, Promtail, and Grafana.

## Environment Configuration

All API containers run with:

- `ASPNETCORE_ENVIRONMENT=Compose`
- `ASPNETCORE_URLS=http://+:8080`

Connection strings are injected through environment variables:

- `ConnectionStrings__BasketDb`
- `ConnectionStrings__OrderDb`
- `ConnectionStrings__LogisticsDb`
- `ConnectionStrings__MessageBroker`

The gateway also runs on port `8080` internally and publishes `5010` externally.

## PostgreSQL Initialization

The PostgreSQL container mounts `docker/postgres/init/01-create-service-databases.sh`. On container initialization it creates:

- `basketdb`
- `orderdb`
- `logisticsdb`

The compose files set:

- `POSTGRES_USER=ecommerce`
- `POSTGRES_PASSWORD=ecommerce`
- `POSTGRES_DB=ecommerce`

The bootstrap database `ecommerce` is used only to connect and create the service databases.

## Service Startup Ordering

Compose uses health checks to delay service startup until infrastructure is ready:

- APIs wait for PostgreSQL and RabbitMQ to become healthy.
- The gateway depends on the three APIs.
- Prometheus depends on the gateway.
- Promtail depends on Loki.
- Grafana depends on Prometheus and Loki.

This does not replace application-level retry logic, but it makes local startup more predictable.

## Observability Configuration

The full compose stack mounts these configuration files:

- `docker/prometheus/prometheus.yml`
- `docker/promtail/config.yml`
- `docker/grafana/provisioning/datasources/datasources.yml`

Prometheus scrapes `/metrics` from:

- `gateway:8080`
- `basket-api:8080`
- `order-api:8080`
- `logistics-api:8080`

Promtail tails Docker container logs and ships them to Loki. Grafana is preconfigured with Prometheus and Loki data sources.

## Local Usage

Typical commands:

```powershell
docker compose up --build
```

```powershell
docker compose -f docker-compose-light.yml up --build
```

Useful endpoints after startup:

- `http://localhost:5010/scalar`
- `http://localhost:5010/console`
- `http://localhost:15672`
- `http://localhost:3000`
- `http://localhost:9090`

## Notes

The Docker runtime path is currently the clearest way to run the whole stack outside Aspire. The compose configuration mirrors the same service boundaries used by `Ecommerce.AppHost`, but replaces Aspire service discovery with explicit container addresses.
