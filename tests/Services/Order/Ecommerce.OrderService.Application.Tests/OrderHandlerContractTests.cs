using Ecommerce.OrderService.Application;
using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.OrderService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.OrderService.Application.Tests;

public sealed class OrderHandlerContractTests
{
    [Fact]
    public async Task CreateOrderFromBasketCheckoutHandler_CreatesPendingOrder()
    {
        await using var dbContext = CreateDbContext(nameof(CreateOrderFromBasketCheckoutHandler_CreatesPendingOrder));
        var handler = new CreateOrderFromBasketCheckoutHandler(dbContext);

        var order = await handler.ExecuteAsync(
            new CreateOrderFromBasketCheckoutCommand(
                Guid.NewGuid(),
                [new CreateOrderLineItem(Guid.NewGuid(), "Keyboard", 2, 25m)],
                50m,
                Guid.NewGuid(),
                Guid.NewGuid()),
            CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Pending.ToString());
        order.FinalTotal.Should().Be(50m);
        (await dbContext.Orders.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateOrderFromBasketCheckoutHandler_WhenCheckoutIsReplayed_ReturnsExistingOrder()
    {
        await using var dbContext = CreateDbContext(nameof(CreateOrderFromBasketCheckoutHandler_WhenCheckoutIsReplayed_ReturnsExistingOrder));
        var handler = new CreateOrderFromBasketCheckoutHandler(dbContext);
        var correlationId = Guid.NewGuid();

        var firstOrder = await handler.ExecuteAsync(
            new CreateOrderFromBasketCheckoutCommand(
                Guid.NewGuid(),
                [new CreateOrderLineItem(Guid.NewGuid(), "Keyboard", 2, 25m)],
                50m,
                correlationId,
                Guid.NewGuid()),
            CancellationToken.None);

        var replayedOrder = await handler.ExecuteAsync(
            new CreateOrderFromBasketCheckoutCommand(
                Guid.NewGuid(),
                [new CreateOrderLineItem(Guid.NewGuid(), "Mouse", 1, 10m)],
                10m,
                correlationId,
                Guid.NewGuid()),
            CancellationToken.None);

        replayedOrder.OrderId.Should().Be(firstOrder.OrderId);
        replayedOrder.CustomerId.Should().Be(firstOrder.CustomerId);
        replayedOrder.ItemsTotal.Should().Be(firstOrder.ItemsTotal);
        (await dbContext.Orders.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetOrderHandler_WhenMissing_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext(nameof(GetOrderHandler_WhenMissing_ThrowsNotFound));
        var handler = new GetOrderHandler(dbContext);

        var act = () => handler.ExecuteAsync(new GetOrderQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<OrderException>()
            .Where(exception => exception.Code == OrderErrorCode.OrderNotFound && exception.Type == OrderErrorType.NotFound);
    }

    [Fact]
    public async Task ConfirmOrderHandler_UpdatesOrderTotals()
    {
        await using var dbContext = CreateDbContext(nameof(ConfirmOrderHandler_UpdatesOrderTotals));
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());
        order.ClearDomainEvents();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new ConfirmOrderHandler(dbContext);

        var result = await handler.ExecuteAsync(
            new ConfirmOrderCommand(order.Id, 10m, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be(OrderStatus.Confirmed.ToString());
        result.FinalTotal.Should().Be(60m);
    }

    [Fact]
    public async Task ConfirmOrderHandler_WhenReplayMatchesCurrentState_ReturnsExistingOrder()
    {
        await using var dbContext = CreateDbContext(nameof(ConfirmOrderHandler_WhenReplayMatchesCurrentState_ReturnsExistingOrder));
        var order = CreateStoredOrder(dbContext);
        order.Confirm(10m, Guid.NewGuid(), Guid.NewGuid());
        await dbContext.SaveChangesAsync(CancellationToken.None);
        order.ClearDomainEvents();
        var handler = new ConfirmOrderHandler(dbContext);

        var result = await handler.ExecuteAsync(
            new ConfirmOrderCommand(order.Id, 10m, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be(OrderStatus.Confirmed.ToString());
        result.ShippingPrice.Should().Be(10m);
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmOrderHandler_WhenReplayContradictsCurrentState_ThrowsConflict()
    {
        await using var dbContext = CreateDbContext(nameof(ConfirmOrderHandler_WhenReplayContradictsCurrentState_ThrowsConflict));
        var order = CreateStoredOrder(dbContext);
        order.Confirm(10m, Guid.NewGuid(), Guid.NewGuid());
        await dbContext.SaveChangesAsync(CancellationToken.None);
        order.ClearDomainEvents();
        var handler = new ConfirmOrderHandler(dbContext);

        var act = () => handler.ExecuteAsync(
            new ConfirmOrderCommand(order.Id, 12m, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<OrderException>()
            .Where(exception => exception.Code == OrderErrorCode.InvalidOrderState && exception.Type == OrderErrorType.Conflict);
    }

    [Fact]
    public async Task CancelOrderHandler_StoresCancellationReason()
    {
        await using var dbContext = CreateDbContext(nameof(CancelOrderHandler_StoresCancellationReason));
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());
        order.ClearDomainEvents();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new CancelOrderHandler(dbContext);

        var result = await handler.ExecuteAsync(
            new CancelOrderCommand(order.Id, "Shipment failed", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be(OrderStatus.Cancelled.ToString());
        result.CancellationReason.Should().Be("Shipment failed");
    }

    [Fact]
    public async Task CancelOrderHandler_WhenReplayMatchesCurrentState_ReturnsExistingOrder()
    {
        await using var dbContext = CreateDbContext(nameof(CancelOrderHandler_WhenReplayMatchesCurrentState_ReturnsExistingOrder));
        var order = CreateStoredOrder(dbContext);
        order.Cancel("Shipment failed", Guid.NewGuid(), Guid.NewGuid());
        await dbContext.SaveChangesAsync(CancellationToken.None);
        order.ClearDomainEvents();
        var handler = new CancelOrderHandler(dbContext);

        var result = await handler.ExecuteAsync(
            new CancelOrderCommand(order.Id, "Shipment failed", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be(OrderStatus.Cancelled.ToString());
        result.CancellationReason.Should().Be("Shipment failed");
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelOrderHandler_WhenReplayContradictsCurrentState_ThrowsConflict()
    {
        await using var dbContext = CreateDbContext(nameof(CancelOrderHandler_WhenReplayContradictsCurrentState_ThrowsConflict));
        var order = CreateStoredOrder(dbContext);
        order.Cancel("Shipment failed", Guid.NewGuid(), Guid.NewGuid());
        await dbContext.SaveChangesAsync(CancellationToken.None);
        order.ClearDomainEvents();
        var handler = new CancelOrderHandler(dbContext);

        var act = () => handler.ExecuteAsync(
            new CancelOrderCommand(order.Id, "Carrier unavailable", Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<OrderException>()
            .Where(exception => exception.Code == OrderErrorCode.InvalidOrderState && exception.Type == OrderErrorType.Conflict);
    }

    private static OrderDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new OrderDbContext(options, new NullDomainEventOutboxMessageFactory());
    }

    private static Order CreateStoredOrder(OrderDbContext dbContext)
    {
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());
        order.ClearDomainEvents();
        dbContext.Orders.Add(order);
        return order;
    }

    private sealed class NullDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents) => [];
    }
}
