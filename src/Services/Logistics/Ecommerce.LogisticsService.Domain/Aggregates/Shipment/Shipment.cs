namespace Ecommerce.LogisticsService.Domain;

public sealed class Shipment : Entity
{
    private const string DefaultCarrier = "DefaultCarrier";
    private const decimal DefaultShippingPrice = 12.50m;

    private Shipment()
    {
    }

    private Shipment(Guid id, Guid orderId, string carrier, decimal shippingPrice, ShipmentStatus status, string? failureReason)
    {
        Id = id;
        OrderId = orderId;
        Carrier = carrier;
        ShippingPrice = shippingPrice;
        Status = status;
        FailureReason = failureReason;
    }

    public Guid OrderId { get; private set; }

    public string Carrier { get; private set; } = string.Empty;

    public decimal ShippingPrice { get; private set; }

    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;

    public string? FailureReason { get; private set; }

    public static Shipment CreateForOrder(
        Guid orderId,
        Guid correlationId,
        Guid? causationId)
    {
        if (orderId == Guid.Empty)
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidOrderId, "orderId", "Order id is required.");
        }

        var shipment = new Shipment(Guid.NewGuid(), orderId, DefaultCarrier, DefaultShippingPrice, ShipmentStatus.Reserved, null);
        shipment.RaiseDomainEvent(new ShipmentReservedDomainEvent(
            shipment.Id,
            shipment.OrderId,
            shipment.Carrier,
            shipment.ShippingPrice,
            shipment.Status.ToString(),
            correlationId,
            causationId));
        shipment.RaiseDomainEvent(new ShipmentRepricedDomainEvent(
            shipment.Id,
            shipment.OrderId,
            shipment.ShippingPrice,
            correlationId,
            causationId));

        return shipment;
    }

    public static Shipment CreateFailed(
        Guid orderId,
        string reason,
        Guid correlationId,
        Guid? causationId)
    {
        if (orderId == Guid.Empty)
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidOrderId, "orderId", "Order id is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidShipmentState, "reason", "Failure reason is required.");
        }

        var shipment = new Shipment(Guid.NewGuid(), orderId, string.Empty, 0m, ShipmentStatus.Pending, null);
        shipment.Fail(reason, correlationId, causationId);
        return shipment;
    }

    public void Fail(string reason, Guid correlationId, Guid? causationId)
    {
        EnsureMutable();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidShipmentState, "reason", "Failure reason is required.");
        }

        Status = ShipmentStatus.Failed;
        FailureReason = reason;
        Carrier = string.Empty;
        ShippingPrice = 0;

        RaiseDomainEvent(new ShipmentFailedDomainEvent(
            OrderId,
            reason,
            correlationId,
            causationId));
    }

    public void Reprice(decimal shippingPrice, Guid correlationId, Guid? causationId)
    {
        if (Status != ShipmentStatus.Reserved)
        {
            throw LogisticsException.Conflict(LogisticsErrorCode.InvalidShipmentState, "Only reserved shipments can be repriced.");
        }

        if (shippingPrice < 0)
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidShippingPrice, "shippingPrice", "Shipping price must be greater than or equal to zero.");
        }

        ShippingPrice = shippingPrice;
        RaiseDomainEvent(new ShipmentRepricedDomainEvent(Id, OrderId, ShippingPrice, correlationId, causationId));
    }

    private void EnsureMutable()
    {
        if (Status == ShipmentStatus.Reserved || Status == ShipmentStatus.Failed)
        {
            throw LogisticsException.Conflict(LogisticsErrorCode.ShipmentAlreadyFinalized, "Shipment has already been finalized.");
        }
    }
}
