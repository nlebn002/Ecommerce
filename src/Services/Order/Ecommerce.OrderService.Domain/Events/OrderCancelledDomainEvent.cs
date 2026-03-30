namespace Ecommerce.OrderService.Domain;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    string Reason,
    string Status,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
