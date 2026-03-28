using Ecommerce.Basket.Domain;

namespace Ecommerce.Basket.Application;

internal static class BasketMappings
{
    public static BasketDto ToDto(this Basket basket)
    {
        return new BasketDto(
            basket.Id,
            basket.CustomerId,
            basket.Status.ToString(),
            basket.Total,
            basket.Items
                .OrderBy(item => item.ProductId, StringComparer.Ordinal)
                .Select(item => new BasketItemDto(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                .ToArray());
    }
}
