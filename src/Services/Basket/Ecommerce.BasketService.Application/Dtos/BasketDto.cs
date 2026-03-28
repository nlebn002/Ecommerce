using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Application;

public sealed record BasketDto(
    Guid BasketId,
    string CustomerId,
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
                .OrderBy(item => item.ProductId, StringComparer.Ordinal)
                .Select(item => item.ToDto())
                .ToArray());
    }
}

