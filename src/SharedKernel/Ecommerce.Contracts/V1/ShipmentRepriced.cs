namespace Ecommerce.Contracts.V1;

public sealed record ShipmentRepriced : IntegrationEvent
{
    public required string OrderId { get; init; }

    public required string ShipmentId { get; init; }

    public decimal ShippingPrice { get; init; }
}
