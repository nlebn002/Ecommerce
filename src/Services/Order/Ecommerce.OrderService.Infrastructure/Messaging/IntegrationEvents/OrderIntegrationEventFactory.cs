using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Infrastructure.Messaging.IntegrationEvents;

public sealed class OrderIntegrationEventFactory
{
    private readonly TimeProvider _timeProvider;

    public OrderIntegrationEventFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Ecommerce.Contracts.V1.OrderCreated Create(OrderCreatedDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.OrderCreated
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            CustomerId = domainEvent.CustomerId.ToString(),
            ItemsTotal = domainEvent.ItemsTotal,
            Status = domainEvent.Status,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }

    public Ecommerce.Contracts.V1.OrderConfirmed Create(OrderConfirmedDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.OrderConfirmed
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            ShippingPrice = domainEvent.ShippingPrice,
            FinalTotal = domainEvent.FinalTotal,
            Status = domainEvent.Status,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }

    public Ecommerce.Contracts.V1.OrderCancelled Create(OrderCancelledDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.OrderCancelled
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            Reason = domainEvent.Reason,
            Status = domainEvent.Status,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }
}
