namespace Ecommerce.Contracts.V1;

public sealed record ShipmentReserved : IntegrationEvent
{
    public required string OrderId { get; init; }

    public required string ShipmentId { get; init; }

    public required string Carrier { get; init; }
}
