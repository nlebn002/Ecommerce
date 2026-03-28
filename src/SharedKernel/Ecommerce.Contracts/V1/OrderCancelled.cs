namespace Ecommerce.Contracts.V1;

public sealed record OrderCancelled : IntegrationEvent
{
    public required string OrderId { get; init; }

    public required string Reason { get; init; }

    public required string Status { get; init; }
}
