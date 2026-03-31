namespace Ecommerce.LogisticsService.Domain;

public sealed record ShipmentFailedDomainEvent(
    Guid OrderId,
    string Reason,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
