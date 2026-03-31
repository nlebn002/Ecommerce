namespace Ecommerce.LogisticsService.Application;

public sealed record ShipmentSummaryDto(
    Guid Id,
    Guid OrderId,
    string Carrier,
    decimal ShippingPrice,
    string Status);
