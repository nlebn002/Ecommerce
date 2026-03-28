namespace Ecommerce.Contracts.V1;

public sealed record BasketCheckedOut : IntegrationEvent
{
    public required Guid BasketId { get; init; }

    public required Guid CustomerId { get; init; }

    public required IReadOnlyList<BasketItem> Items { get; init; }

    public decimal ItemsTotal { get; init; }
}
