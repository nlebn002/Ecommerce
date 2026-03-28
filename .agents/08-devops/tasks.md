# DevOps - Tasks

## Task 1: Write a Dockerfile (One Per Service)
**Description**: Create a multi-stage Dockerfile for each microservice.

**Example** (`docker/Dockerfile.basket`):
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/ECommerce.Basket/ ./ECommerce.Basket/
COPY src/ECommerce.ServiceDefaults/ ./ECommerce.ServiceDefaults/
RUN dotnet publish ECommerce.Basket/ECommerce.Basket.csproj \
    -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.Basket.dll"]
```

Repeat for Dockerfile.order, Dockerfile.logistics, and Dockerfile.gateway.

## Task 2: Write Docker Compose
Description: Define docker-compose.yml to run all services, databases, and RabbitMQ.

```yaml
services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  basket-db:
    image: postgres:16
    environment:
      POSTGRES_DB: basketdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres

  order-db:
    image: postgres:16
    environment:
      POSTGRES_DB: orderdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres

  logistics-db:
    image: postgres:16
    environment:
      POSTGRES_DB: logisticsdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres

  basket:
    build:
      context: .
      dockerfile: docker/Dockerfile.basket
    environment:
      RABBITMQ_HOST: rabbitmq
      BASKET_DB_CONN: Host=basket-db;Database=basketdb;Username=postgres;Password=postgres
    depends_on:
      - rabbitmq
      - basket-db

  order:
    build:
      context: .
      dockerfile: docker/Dockerfile.order
    environment:
      RABBITMQ_HOST: rabbitmq
      ORDER_DB_CONN: Host=order-db;Database=orderdb;Username=postgres;Password=postgres
    depends_on:
      - rabbitmq
      - order-db

  logistics:
    build:
      context: .
      dockerfile: docker/Dockerfile.logistics
    environment:
      RABBITMQ_HOST: rabbitmq
      LOGISTICS_DB_CONN: Host=logistics-db;Database=logisticsdb;Username=postgres;Password=postgres
    depends_on:
      - rabbitmq
      - logistics-db

  gateway:
    build:
      context: .
      dockerfile: docker/Dockerfile.gateway
    ports:
      - "8080:8080"
    environment:
      BASKET_URL: http://basket:8080
      ORDER_URL: http://order:8080
      LOGISTICS_URL: http://logistics:8080
    depends_on:
      - basket
      - order
      - logistics
```

## Task 3: Write the CI/CD Pipeline
Description: GitHub Actions workflow that builds, tests, and deploys on push to main.

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore
        run: dotnet restore ECommerce.sln

      - name: Build
        run: dotnet build ECommerce.sln --no-restore -c Release

      - name: Unit Tests
        run: |
          dotnet test tests/ECommerce.Basket.Tests/ --no-build -c Release
          dotnet test tests/ECommerce.Order.Tests/ --no-build -c Release
          dotnet test tests/ECommerce.Logistics.Tests/ --no-build -c Release

  integration-tests:
    runs-on: ubuntu-latest
    needs: build-and-test
    services:
      rabbitmq:
        image: rabbitmq:3
        ports:
          - 5672:5672
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: postgres
        ports:
          - 5432:5432
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Integration Tests
        run: dotnet test tests/ECommerce.Integration.Tests/ -c Release
        env:
          RABBITMQ_HOST: localhost
          POSTGRES_HOST: localhost

  deploy:
    runs-on: ubuntu-latest
    needs: integration-tests
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4

      - name: Build Images
        run: |
          docker compose build

      - name: Push Images
        run: |
          echo "$${{ secrets.REGISTRY_PASSWORD }}" | docker login -u $${{ secrets.REGISTRY_USER }} --password-stdin
          docker compose push

      - name: Deploy
        run: |
          ssh ${{ secrets.DEPLOY_HOST }} "cd /app && docker compose pull && docker compose up -d"
```

## Task 4: Add Health Checks to Compose
Description: Add health checks so services wait for dependencies to be ready.

```yaml
rabbitmq:
    image: rabbitmq:3-management
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 10s
      timeout: 5s
      retries: 5

  basket-db:
    image: postgres:16
    healthcheck:
      test: pg_isready -U postgres
      interval: 5s
      timeout: 3s
      retries: 5

  basket:
    build:
      context: .
      dockerfile: docker/Dockerfile.basket
    depends_on:
      rabbitmq:
        condition: service_healthy
      basket-db:
        condition: service_healthy
    healthcheck:
      test: curl -f http://localhost:8080/health || exit 1
      interval: 10s
      timeout: 5s
      retries: 3
```

## Task 5: Verify the Full Pipeline
Description: Confirm that a push to main builds, tests, and deploys successfully.

Steps:

Push a commit to main
Verify in GitHub Actions:
✅ build-and-test — all three unit test projects pass
✅ integration-tests — event flow tests pass with real RabbitMQ + Postgres
✅ deploy — images built, pushed, and deployed
After deploy, verify:
GET http://host:8080/health returns healthy
POST http://host:8080/basket/test/checkout triggers the full event flow
RabbitMQ management UI shows messages flowing through queues
Expected Pipeline Result:

build-and-test     ✅ passed (45s)
  ├── restore
  ├── build
  └── unit tests (3/3 passed)

integration-tests  ✅ passed (90s)
  └── event flow tests (3/3 passed)

deploy             ✅ passed (60s)
  ├── docker compose build
  ├── docker compose push
  └── ssh deploy + restart

  ---

Five tasks covering the full DevOps pipeline:

| Task | What It Does |
|------|-------------|
| 1 | Dockerfile per service (multi-stage build) |
| 2 | Docker Compose to run everything together |
| 3 | GitHub Actions CI/CD (build → unit test → integration test → deploy) |
| 4 | Health checks so services start in the right order |
| 5 | Verify the full pipeline works end-to-end |