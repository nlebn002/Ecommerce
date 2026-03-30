namespace Ecommerce.OrderService.Domain;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal ItemsTotal,
    string Status,
    Guid CorrelationId,
    Guid? CausationId) : IDomainEvent;
