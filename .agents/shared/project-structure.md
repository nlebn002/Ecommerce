# Project Structure

```text
Ecommerce/
├── src/
│   ├── SharedKernel/
│   │   ├── Ecommerce.Contracts/
│   │   └── Ecommerce.Common/
│   ├── Services/
│   │   ├── Basket/
│   │   │   ├── Ecommerce.Basket.Domain/
│   │   │   ├── Ecommerce.Basket.Application/
│   │   │   ├── Ecommerce.Basket.Infrastructure/
│   │   │   └── Ecommerce.Basket.Api/
│   │   ├── Order/
│   │   │   ├── Ecommerce.Order.Domain/
│   │   │   ├── Ecommerce.Order.Application/
│   │   │   ├── Ecommerce.Order.Infrastructure/
│   │   │   └── Ecommerce.Order.Api/
│   │   └── Logistics/
│   │       ├── Ecommerce.Logistics.Domain/
│   │       ├── Ecommerce.Logistics.Application/
│   │       ├── Ecommerce.Logistics.Infrastructure/
│   │       └── Ecommerce.Logistics.Api/
│   ├── Gateway/
│   │   └── Ecommerce.Gateway/
│   └── Orchestration/
│       ├── Ecommerce.AppHost/
│       └── Ecommerce.ServiceDefaults/
├── tests/
├── Directory.Build.props
├── Directory.Packages.props
├── docker-compose.yml
├── prometheus.yml
└── Ecommerce.sln
```

## Rules

- SharedKernel is the only shared code dependency
- Each service owns its database
- Domain -> Application -> Infrastructure -> Api
- Gateway is the public edge
