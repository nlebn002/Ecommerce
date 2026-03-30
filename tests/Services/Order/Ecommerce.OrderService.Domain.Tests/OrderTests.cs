using Ecommerce.OrderService.Domain;
using FluentAssertions;
using Xunit;

namespace Ecommerce.OrderService.Domain.Tests;

public sealed class OrderTests
{
    [Fact]
    public void Create_SetsPendingState_AndRaisesCreatedEvent()
    {
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var order = Order.Create(
            customerId,
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            correlationId,
            Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Pending);
        order.FinalTotal.Should().Be(50m);
        order.Items.Should().ContainSingle();

        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>()
            .Which;

        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.ItemsTotal.Should().Be(50m);
        domainEvent.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void Confirm_SetsConfirmedState_AndRaisesConfirmedEvent()
    {
        var order = CreatePendingOrder();

        order.Confirm(12.5m, Guid.NewGuid(), Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ShippingPrice.Should().Be(12.5m);
        order.FinalTotal.Should().Be(62.5m);
        order.DomainEvents.Should().ContainSingle(eventItem => eventItem is OrderConfirmedDomainEvent);
    }

    [Fact]
    public void Cancel_SetsCancelledState_AndRaisesCancelledEvent()
    {
        var order = CreatePendingOrder();

        order.Cancel("Shipment failed", Guid.NewGuid(), Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Shipment failed");
        order.DomainEvents.Should().ContainSingle(eventItem => eventItem is OrderCancelledDomainEvent);
    }

    [Fact]
    public void Confirm_WhenOrderAlreadyCancelled_ThrowsConflict()
    {
        var order = CreatePendingOrder();
        order.Cancel("Shipment failed", Guid.NewGuid(), Guid.NewGuid());
        order.ClearDomainEvents();

        var act = () => order.Confirm(10m, Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<OrderException>()
            .Where(exception => exception.Code == OrderErrorCode.ShipmentAlreadyProcessed && exception.Type == OrderErrorType.Conflict);
    }

    [Fact]
    public void Cancel_WhenReasonMissing_ThrowsValidation()
    {
        var order = CreatePendingOrder();

        var act = () => order.Cancel(string.Empty, Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<OrderException>()
            .Where(exception => exception.Code == OrderErrorCode.InvalidOrderState && exception.Type == OrderErrorType.Validation);
    }

    private static Order CreatePendingOrder()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());
        order.ClearDomainEvents();
        return order;
    }
}
