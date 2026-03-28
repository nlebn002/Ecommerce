using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed record BasketDto(
    Guid BasketId,
    Guid CustomerId,
    string Status,
    decimal Total,
    IReadOnlyList<BasketItemDto> Items);

internal static class BasketDtoMappings
{
    public static BasketDto ToDto(this Basket basket)
    {
        return new BasketDto(
            basket.Id,
            basket.CustomerId,
            basket.Status.ToString(),
            basket.Total,
            basket.Items
                .Where(item => !item.IsDeleted)
                .OrderBy(item => item.ProductId)
                .Select(item => item.ToDto())
                .ToArray());
    }
}

