namespace Ecommerce.LogisticsService.Domain;

public sealed record ShipmentReservedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    string Carrier,
    decimal ShippingPrice,
    string Status,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
