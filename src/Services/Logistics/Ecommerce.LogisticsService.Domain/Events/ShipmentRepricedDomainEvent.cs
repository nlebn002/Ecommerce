namespace Ecommerce.LogisticsService.Domain;

public sealed record ShipmentRepricedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    decimal ShippingPrice,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
