# Architect Agent - Context

## Shared Knowledge

You have access to these shared files. Read and follow them:

- `shared/plan.md`
- `shared/tech-stack.md`
- `shared/coding-standards.md`
- `shared/project-structure.md`

## What Exists Before You

The repository already contains:

- `Ecommerce.AppHost`
- `Ecommerce.ServiceDefaults`
- Git baseline files such as `.gitignore`

Your job is to build around those existing projects, not replace them.

## What Comes After You

| Next Agent | What They Need From You |
|---|---|
| Contracts | `Ecommerce.Contracts.csproj` with the correct target framework |
| Common | `Ecommerce.Common.csproj` referencing Contracts |
| Basket / Order / Logistics | Four `.csproj` files each with correct references |
| Gateway | `Ecommerce.Gateway.csproj` with YARP packages |
| Aspire | Existing AppHost and ServiceDefaults wired to the new runnable projects |
| Testing | Test projects referencing the correct service projects |
| DevOps | Working compose, Dockerfiles, and CI assets |

## Infrastructure Services

| Service | Image | Ports |
|---|---|---|
| PostgreSQL | `postgres:17` | 5432 |
| RabbitMQ | `rabbitmq:4-management` | 5672, 15672 |
| Redis | `redis:7` | 6379 |
| Prometheus | `prom/prometheus` | 9090 |
| Grafana | `grafana/grafana` | 3000 |

## Default Service Ports

| Service | Port |
|---|---|
| Basket.Api | 5100 |
| Order.Api | 5200 |
| Logistics.Api | 5300 |
| Gateway | 5000 |
| Aspire Dashboard | 18888 |
