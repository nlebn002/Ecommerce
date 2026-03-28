# Testing - Context

## Test Pyramid
    ┌───────┐
    │  E2E  │    Full flow: checkout → order → shipment → delivered
    ├───────┤
    │ Integ │    Real broker + DB: verify events publish and consume
    ├───────┤
    │ Unit  │    Isolated: service logic with mocked dependencies
    └───────┘

## Test Project Structure
ecommerce-solution/
├── tests/
│   ├── ECommerce.Basket.Tests/         ← Basket unit tests
│   ├── ECommerce.Order.Tests/          ← Order unit tests
│   ├── ECommerce.Logistics.Tests/      ← Logistics unit tests
│   └── ECommerce.Integration.Tests/    ← Integration + E2E tests (uses Aspire)

## What Each Layer Tests

| Layer       | Infrastructure | What's Verified                                    |
|-------------|----------------|----------------------------------------------------|
| Unit        | All mocked     | Business logic: totals, status transitions, validation |
| Integration | Real (Aspire)  | Events are published and consumed correctly         |
| E2E         | Real (Aspire)  | Full flow from HTTP request to final state          |

## Key Events to Test

| Event              | Producer   | Consumer   | What to Assert                          |
|--------------------|------------|------------|-----------------------------------------|
| basket.checked_out | Basket     | Order      | Order is created with correct items     |
| order.confirmed    | Order      | Logistics  | Shipment is created for the order       |
| order.cancelled    | Order      | Logistics  | Pending shipment is cancelled           |
| shipment.shipped   | Logistics  | Order      | Order status updated to SHIPPED         |
| shipment.delivered | Logistics  | Order      | Order status updated to DELIVERED       |

## Aspire Integration Testing Setup

```csharp
// Uses Microsoft.Extensions.ServiceDefaults.Testing
// Spins up real RabbitMQ, Postgres, and all services

public class IntegrationTestBase : IAsyncLifetime
{
    private DistributedApplication _app;
    protected HttpClient GatewayClient;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ECommerce_AppHost>();

        _app = await builder.BuildAsync();
        await _app.StartAsync();

        GatewayClient = _app.CreateHttpClient("gateway");
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
```

## Conventions
Naming: MethodName_Scenario_ExpectedResult
Arrange-Act-Assert pattern for all tests
Polling with timeout for async event assertions (events are not instant)

```csharp
// Helper: wait for an async condition
async Task WaitForCondition(Func<Task<bool>> condition, TimeSpan timeout)
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        if (await condition()) return;
        await Task.Delay(250);
    }
    throw new TimeoutException("Condition not met within timeout.");
}
```

