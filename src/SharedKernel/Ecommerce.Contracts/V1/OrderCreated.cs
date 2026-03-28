namespace Ecommerce.Contracts.V1;

public sealed record OrderCreated : IntegrationEvent
{
    public required string OrderId { get; init; }

    public required string CustomerId { get; init; }

    public decimal ItemsTotal { get; init; }

    public required string Status { get; init; }
}
