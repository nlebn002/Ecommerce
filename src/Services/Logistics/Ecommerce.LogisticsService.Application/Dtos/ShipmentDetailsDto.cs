namespace Ecommerce.LogisticsService.Application;

public sealed record ShipmentDetailsDto(
    Guid Id,
    Guid OrderId,
    string Carrier,
    decimal ShippingPrice,
    string Status,
    string? FailureReason);
