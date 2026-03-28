using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed record BasketItemDto(string ProductId, string ProductName, int Quantity, decimal UnitPrice);

internal static class BasketItemDtoMappings
{
    public static BasketItemDto ToDto(this BasketItem basketItem)
    {
        return new BasketItemDto(
            basketItem.ProductId,
            basketItem.ProductName,
            basketItem.Quantity,
            basketItem.UnitPrice);
    }
}

