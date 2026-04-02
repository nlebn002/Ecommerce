# Workflow

## Development Model

This solution is organized as three business services behind a gateway:

- basket
- order
- logistics

The normal development workflow is service-oriented:

1. Make changes inside one service boundary.
2. Keep domain, application, API, and infrastructure changes together for that service.
3. Use shared contracts only for cross-service integration concerns.
4. Verify the resulting HTTP and messaging flow through the gateway or Docker/Aspire orchestration.

## Typical Business Flow

The most important end-to-end workflow is checkout to order finalization:

1. Create a basket.
2. Add or update basket items.
3. Checkout the basket.
4. Wait for asynchronous processing:
   - basket publishes checkout event
   - order creates the order
   - logistics reserves shipment
   - order is confirmed or cancelled
5. Query order and shipment state through the gateway.

Client-facing read paths stay synchronous over HTTP. Cross-service write orchestration happens asynchronously through RabbitMQ.

## Local Run Options

There are two main local runtime options.

### Option 1: Aspire

Use the app host when you want .NET-native orchestration and service discovery:

```powershell
dotnet run --project .\src\Orchestration\Ecommerce.AppHost
```

This provisions PostgreSQL, RabbitMQ, the three APIs, and the gateway.

### Option 2: Docker Compose

Use compose when you want containerized local execution:

```powershell
docker compose up --build
```

Or the smaller stack:

```powershell
docker compose -f docker-compose-light.yml up --build
```

## API Development Workflow

For changes to an HTTP feature:

1. Update domain rules if the business invariant changes.
2. Update or add an application handler.
3. Update the API endpoint or request validator.
4. If persistence changes are required, add an EF Core migration.
5. Run or update the relevant tests.
6. Validate the endpoint through:
   - gateway route
   - service-local Scalar UI
   - generated OpenAPI document

Current versioned API groups are:

- basket: `/api/v1/baskets`
- order: `/api/v1/orders`
- logistics: `/api/v1/shipments`

Gateway-facing paths are:

- `/basket/v1/baskets`
- `/orders/v1/orders`
- `/logistics/v1/shipments`

## Database Change Workflow

When changing a service schema:

1. Update the domain and persistence model.
2. Create a migration with the service-specific script in `scripts/`.
3. Review the generated migration.
4. Start the service or full stack so pending migrations are applied.

Examples:

```powershell
.\scripts\add-basket-migration.ps1
```

```powershell
.\scripts\add-order-migration.ps1
```

```powershell
.\scripts\add-logistics-migration.ps1
```

## Testing Workflow

The test projects are grouped by service and layer. A practical sequence is:

1. Run the affected service test projects first.
2. Run the full test suite before merging.
3. If the change affects messaging or routing, verify it against a running stack.

Typical command:

```powershell
dotnet test .\Ecommerce.slnx
```

## Operational Checks

After starting the system, the most useful checks are:

- gateway status: `http://localhost:5010/`
- docs: `http://localhost:5010/scalar`
- demo console: `http://localhost:5010/console`
- liveness: `/health/live`
- readiness: `/health/ready`
- metrics: `/metrics`
- RabbitMQ UI: `http://localhost:15672`
- Grafana: `http://localhost:3000`

## Recommended Change Boundaries

Keep changes narrow and explicit:

- prefer changes inside one service unless a contract genuinely has to move
- treat `Ecommerce.Contracts` as a versioned integration boundary
- do not couple services through another service's database
- prefer publishing events over introducing direct service-to-service writes
