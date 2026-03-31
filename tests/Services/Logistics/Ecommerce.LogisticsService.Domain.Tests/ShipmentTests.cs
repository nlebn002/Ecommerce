using Ecommerce.LogisticsService.Domain;
using FluentAssertions;
using Xunit;

namespace Ecommerce.LogisticsService.Domain.Tests;

public sealed class ShipmentTests
{
    [Fact]
    public void CreateForOrder_ReservesShipmentAndRaisesEvents()
    {
        var shipment = Shipment.CreateForOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        shipment.Status.Should().Be(ShipmentStatus.Reserved);
        shipment.Carrier.Should().Be("DefaultCarrier");
        shipment.ShippingPrice.Should().Be(12.50m);
        shipment.DomainEvents.Should().HaveCount(2);
        shipment.DomainEvents.Should().ContainItemsAssignableTo<ShipmentReservedDomainEvent>();
        shipment.DomainEvents.Should().ContainItemsAssignableTo<ShipmentRepricedDomainEvent>();
    }

    [Fact]
    public void CreateForOrder_WithEmptyOrderId_ThrowsValidation()
    {
        var act = () => Shipment.CreateForOrder(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<LogisticsException>()
            .Where(exception => exception.Code == LogisticsErrorCode.InvalidOrderId && exception.Type == LogisticsErrorType.Validation);
    }

    [Fact]
    public void CreateFailed_SetsFailedStatus()
    {
        var shipment = Shipment.CreateFailed(Guid.NewGuid(), "No shipment available", Guid.NewGuid(), Guid.NewGuid());

        shipment.Status.Should().Be(ShipmentStatus.Failed);
        shipment.FailureReason.Should().Be("No shipment available");
        shipment.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ShipmentFailedDomainEvent>();
    }

    [Fact]
    public void Reprice_AfterFailure_ThrowsConflict()
    {
        var shipment = Shipment.CreateFailed(Guid.NewGuid(), "No shipment available", Guid.NewGuid(), Guid.NewGuid());

        var act = () => shipment.Reprice(5m, Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<LogisticsException>()
            .Where(exception => exception.Code == LogisticsErrorCode.InvalidShipmentState && exception.Type == LogisticsErrorType.Conflict);
    }
}
