namespace Ecommerce.Contracts.V1;

public sealed record ShipmentFailed : IntegrationEvent
{
    public required string OrderId { get; init; }

    public required string Reason { get; init; }
}
