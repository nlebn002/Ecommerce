namespace Ecommerce.Basket.Application;

public sealed record BasketItemDto(string ProductId, string ProductName, int Quantity, decimal UnitPrice);
