using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed record OrderSummaryDto(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    decimal ItemsTotal,
    decimal ShippingPrice,
    decimal FinalTotal);

internal static class OrderSummaryDtoMappings
{
    public static OrderSummaryDto ToSummaryDto(this Order order)
    {
        return new(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.ItemsTotal,
            order.ShippingPrice,
            order.FinalTotal);
    }
}
