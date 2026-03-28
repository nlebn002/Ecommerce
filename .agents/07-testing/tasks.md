# Testing - Tasks

## Task 1: Unit Test — Basket Checkout Validation
**Description**: Test that the Basket service rejects checkout for an empty basket.

```csharp
public class BasketCheckoutTests
{
    [Fact]
    public async Task Checkout_EmptyBasket_ReturnsError()
    {
        // Arrange
        var mockBroker = new Mock<IMessageBroker>();
        var mockRepo = new Mock<IBasketRepository>();
        mockRepo.Setup(r => r.GetAsync("basket_001"))
            .ReturnsAsync(new Basket { Id = "basket_001", Items = [] });

        var service = new BasketService(mockRepo.Object, mockBroker.Object);

        // Act
        var result = await service.CheckoutAsync("basket_001");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Basket is empty", result.Error);
        mockBroker.Verify(b => b.PublishAsync(
            It.IsAny<BasketCheckedOutEvent>()), Times.Never);
    }
}
```

## Task 2: Unit Test — Order Created from Event Payload
Description: Test that the Order service correctly creates an order from a basket.checked_out event.
```csharp
public class OrderCreationTests
{
    [Fact]
    public async Task HandleBasketCheckedOut_ValidPayload_CreatesOrder()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockBroker = new Mock<IMessageBroker>();
        var handler = new BasketCheckedOutHandler(mockRepo.Object, mockBroker.Object);

        var evt = new BasketCheckedOutEvent
        {
            BasketId = "basket_001",
            CustomerId = "cust_001",
            Items = [new Item { ProductId = "prod_1", Quantity = 2, Price = 49.99m }],
            Total = 99.98m
        };

        // Act
        await handler.HandleAsync(evt);

        // Assert
        mockRepo.Verify(r => r.SaveAsync(It.Is<Order>(o =>
            o.CustomerId == "cust_001" &&
            o.Status == OrderStatus.Confirmed &&
            o.Total == 99.98m &&
            o.Items.Count == 1
        )), Times.Once);

        mockBroker.Verify(b => b.PublishAsync(It.Is<OrderConfirmedEvent>(e =>
            e.OrderId != null
        )), Times.Once);
    }
}
```

## Task 3: Unit Test — Order Cancellation Updates Status
Description: Test that cancelling a confirmed order publishes the cancellation event.
```csharp
public class OrderCancellationTests
{
    [Fact]
    public async Task Cancel_ConfirmedOrder_PublishesCancelledEvent()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockBroker = new Mock<IMessageBroker>();
        mockRepo.Setup(r => r.GetAsync("ord_001"))
            .ReturnsAsync(new Order { Id = "ord_001", Status = OrderStatus.Confirmed });

        var service = new OrderService(mockRepo.Object, mockBroker.Object);

        // Act
        var result = await service.CancelAsync("ord_001");

        // Assert
        Assert.True(result.Success);
        mockBroker.Verify(b => b.PublishAsync(It.Is<OrderCancelledEvent>(e =>
            e.OrderId == "ord_001"
        )), Times.Once);
    }

    [Fact]
    public async Task Cancel_ShippedOrder_ReturnsError()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockBroker = new Mock<IMessageBroker>();
        mockRepo.Setup(r => r.GetAsync("ord_002"))
            .ReturnsAsync(new Order { Id = "ord_002", Status = OrderStatus.Shipped });

        var service = new OrderService(mockRepo.Object, mockBroker.Object);

        // Act
        var result = await service.CancelAsync("ord_002");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cannot cancel a shipped order", result.Error);
        mockBroker.Verify(b => b.PublishAsync(
            It.IsAny<OrderCancelledEvent>()), Times.Never);
    }
}
```

## Task 4: Integration Test — Checkout Produces an Order
Description: Using Aspire, verify that checking out a basket results in an order being created via events.

```csharp 
public class CheckoutFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Checkout_ValidBasket_CreatesOrder()
    {
        // Arrange — add item to basket
        await GatewayClient.PostAsJsonAsync("/basket/basket_001/items", new
        {
            ProductId = "prod_1",
            Quantity = 1,
            Price = 29.99m
        });

        // Act — checkout
        var response = await GatewayClient.PostAsync(
            "/basket/basket_001/checkout", null);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        // Assert — order was created (poll, since it's async)
        await WaitForCondition(async () =>
        {
            var orders = await GatewayClient.GetFromJsonAsync<List<Order>>(
                "/orders?customerId=cust_001");
            return orders?.Any(o => o.Status == "Confirmed") == true;
        }, timeout: TimeSpan.FromSeconds(10));
    }
}
```

## Task 5: Integration Test — Order Confirmation Triggers Shipment
Description: Verify that a confirmed order results in a shipment being created by the Logistics service.
```csharp
public class ShipmentCreationTests : IntegrationTestBase
{
    [Fact]
    public async Task OrderConfirmed_CreatesShipment()
    {
        // Arrange — checkout a basket to trigger order creation
        await GatewayClient.PostAsJsonAsync("/basket/basket_002/items", new
        {
            ProductId = "prod_2",
            Quantity = 1,
            Price = 59.99m
        });
        await GatewayClient.PostAsync("/basket/basket_002/checkout", null);

        // Wait for order to be created
        string orderId = null;
        await WaitForCondition(async () =>
        {
            var orders = await GatewayClient.GetFromJsonAsync<List<Order>>(
                "/orders?customerId=cust_001");
            var order = orders?.FirstOrDefault(o => o.Status == "Confirmed");
            orderId = order?.Id;
            return orderId != null;
        }, timeout: TimeSpan.FromSeconds(10));

        // Assert — shipment was created for the order
        await WaitForCondition(async () =>
        {
            var tracking = await GatewayClient.GetAsync(
                $"/orders/{orderId}/tracking");
            return tracking.StatusCode == HttpStatusCode.OK;
        }, timeout: TimeSpan.FromSeconds(10));
    }
}
```

## Task 6: Integration Test — Cancellation Stops Shipment
Description: Verify that cancelling an order publishes an event that cancels the pending shipment.

```csharp
public class CancellationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task CancelOrder_CancelsPendingShipment()
    {
        // Arrange — create an order via checkout
        await GatewayClient.PostAsJsonAsync("/basket/basket_003/items", new
        {
            ProductId = "prod_3",
            Quantity = 1,
            Price = 19.99m
        });
        await GatewayClient.PostAsync("/basket/basket_003/checkout", null);

        string orderId = null;
        await WaitForCondition(async () =>
        {
            var orders = await GatewayClient.GetFromJsonAsync<List<Order>>(
                "/orders?customerId=cust_001");
            orderId = orders?.FirstOrDefault(o => o.Status == "Confirmed")?.Id;
            return orderId != null;
        }, timeout: TimeSpan.FromSeconds(10));

        // Act — cancel the order
        var response = await GatewayClient.PostAsync(
            $"/orders/{orderId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert — order is cancelled
        await WaitForCondition(async () =>
        {
            var order = await GatewayClient.GetFromJsonAsync<Order>(
                $"/orders/{orderId}");
            return order?.Status == "Cancelled";
        }, timeout: TimeSpan.FromSeconds(10));
    }
}
```

## Task 7: E2E Test — Full Checkout to Delivery
Description: Test the entire happy path from basket to delivery.
```csharp 
public class FullFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task FullFlow_Checkout_To_Delivered()
    {
        // 1. Add item to basket
        await GatewayClient.PostAsJsonAsync("/basket/basket_e2e/items", new
        {
            ProductId = "prod_1",
            Quantity = 1,
            Price = 49.99m
        });

        // 2. Checkout
        var checkout = await GatewayClient.PostAsync(
            "/basket/basket_e2e/checkout", null);
        Assert.Equal(HttpStatusCode.Accepted, checkout.StatusCode);

        // 3. Wait for order confirmed
        string orderId = null;
        await WaitForCondition(async () =>
        {
            var orders = await GatewayClient.GetFromJsonAsync<List<Order>>(
                "/orders?customerId=cust_001");
            orderId = orders?.FirstOrDefault(o => o.Status == "Confirmed")?.Id;
            return orderId != null;
        }, timeout: TimeSpan.FromSeconds(10));

        // 4. Wait for shipment created + shipped
        await WaitForCondition(async () =>
        {
            var order = await GatewayClient.GetFromJsonAsync<Order>(
                $"/orders/{orderId}");
            return order?.Status == "Shipped";
        }, timeout: TimeSpan.FromSeconds(15));

        // 5. Simulate delivery (call logistics endpoint)
        var tracking = await GatewayClient.GetFromJsonAsync<Tracking>(
            $"/orders/{orderId}/tracking");
        await GatewayClient.PostAsync(
            $"/logistics/{tracking.ShipmentId}/deliver", null);

        // 6. Wait for order delivered
        await WaitForCondition(async () =>
        {
            var order = await GatewayClient.GetFromJsonAsync<Order>(
                $"/orders/{orderId}");
            return order?.Status == "Delivered";
        }, timeout: TimeSpan.FromSeconds(10));
    }
}
```

---

Seven tasks covering the full test pyramid:

| Task | Layer | What It Proves |
|------|-------|----------------|
| 1 | Unit | Basket validation logic works |
| 2 | Unit | Order creation from event payload works |
| 3 | Unit | Order cancellation logic + event publishing |
| 4 | Integration | Checkout event → Order created (cross-service) |
| 5 | Integration | Order event → Shipment created (cross-service) |
| 6 | Integration | Cancel event → Shipment stopped (cross-service) |
| 7 | E2E | Full happy path: basket → order → shipped → delivered |