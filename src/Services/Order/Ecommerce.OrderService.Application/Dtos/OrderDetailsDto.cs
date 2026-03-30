using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed record OrderDetailsDto(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    decimal ItemsTotal,
    decimal ShippingPrice,
    decimal FinalTotal,
    string? CancellationReason,
    IReadOnlyList<OrderLineItemDto> Items);

internal static class OrderDetailsDtoMappings
{
    public static OrderDetailsDto ToDetailsDto(this Order order)
    {
        return new(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.ItemsTotal,
            order.ShippingPrice,
            order.FinalTotal,
            order.CancellationReason,
            order.Items
                .Where(item => !item.IsDeleted)
                .OrderBy(item => item.ProductId)
                .Select(item => item.ToDto())
                .ToArray());
    }
}
