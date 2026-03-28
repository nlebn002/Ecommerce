# DevOps - Context

## Repository Structure

```text
Ecommerce/
├── src/
│   ├── SharedKernel/
│   ├── Services/
│   ├── Gateway/
│   └── Orchestration/
├── tests/
├── docker-compose.yml
├── prometheus.yml
├── .github/workflows/
└── Ecommerce.sln
```

## Deployment Shape

- Basket, Order, Logistics, and Gateway run as separate containers
- RabbitMQ provides messaging
- PostgreSQL is provisioned per service
- Redis is available for caching and idempotency support
- Prometheus and Grafana support observability

## CI Pipeline Stages

```text
Build -> Unit Tests -> Integration Tests -> Container Build
```

## Expected Environment Variables

| Variable | Service | Purpose |
|---|---|---|
| `RABBITMQ_HOST` | All | Broker hostname |
| `RABBITMQ_USER` | All | Broker username |
| `RABBITMQ_PASS` | All | Broker password |
| `BASKET_DB_CONN` | Basket | Basket database connection string |
| `ORDER_DB_CONN` | Order | Order database connection string |
| `LOGISTICS_DB_CONN` | Logistics | Logistics database connection string |
| `REDIS_CONN` | Relevant services | Redis connection string |
| `BASKET_URL` | Gateway | Basket service internal URL |
| `ORDER_URL` | Gateway | Order service internal URL |
| `LOGISTICS_URL` | Gateway | Logistics service internal URL |
