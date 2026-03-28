namespace Ecommerce.Contracts.V1;

public sealed record BasketItem
{
    public required string ProductId { get; init; }

    public required string ProductName { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}
