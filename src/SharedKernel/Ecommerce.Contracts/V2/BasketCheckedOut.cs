namespace Ecommerce.Contracts.V2;

public sealed record BasketCheckedOut : IntegrationEvent
{
    public required string BasketId { get; init; }

    public required string CustomerId { get; init; }

    public required IReadOnlyList<BasketItem> Items { get; init; }

    public decimal ItemsTotal { get; init; }

    public override string Version { get; init; } = "2.0";
}
