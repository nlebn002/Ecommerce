namespace Ecommerce.Basket.Application;

public sealed record BasketDto(
    string BasketId,
    string CustomerId,
    string Status,
    decimal Total,
    IReadOnlyList<BasketItemDto> Items);
