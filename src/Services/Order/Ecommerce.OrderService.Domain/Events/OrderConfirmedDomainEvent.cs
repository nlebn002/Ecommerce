namespace Ecommerce.OrderService.Domain;

public sealed record OrderConfirmedDomainEvent(
    Guid OrderId,
    decimal ShippingPrice,
    decimal FinalTotal,
    string Status,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
