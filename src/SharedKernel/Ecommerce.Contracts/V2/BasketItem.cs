namespace Ecommerce.Contracts.V2;

public sealed record BasketItem
{
    public required string ProductId { get; init; }

    public required string ProductName { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public string? Sku { get; init; }

    public decimal? Weight { get; init; }
}
