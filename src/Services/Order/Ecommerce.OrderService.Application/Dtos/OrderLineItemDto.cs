using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed record OrderLineItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

internal static class OrderLineItemDtoMappings
{
    public static OrderLineItemDto ToDto(this OrderItem item)
    {
        return new(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
    }
}
