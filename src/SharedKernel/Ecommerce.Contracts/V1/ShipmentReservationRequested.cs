namespace Ecommerce.Contracts.V1;

public sealed record ShipmentReservationRequested : IntegrationEvent
{
    public required string OrderId { get; init; }
}
