using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Infrastructure.Messaging.IntegrationEvents;

public sealed class LogisticsIntegrationEventFactory
{
    private readonly TimeProvider _timeProvider;

    public LogisticsIntegrationEventFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Ecommerce.Contracts.V1.ShipmentReserved Create(ShipmentReservedDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.ShipmentReserved
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            ShipmentId = domainEvent.ShipmentId.ToString(),
            Carrier = domainEvent.Carrier,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }

    public Ecommerce.Contracts.V1.ShipmentRepriced Create(ShipmentRepricedDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.ShipmentRepriced
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            ShipmentId = domainEvent.ShipmentId.ToString(),
            ShippingPrice = domainEvent.ShippingPrice,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }

    public Ecommerce.Contracts.V1.ShipmentFailed Create(ShipmentFailedDomainEvent domainEvent)
    {
        return new Ecommerce.Contracts.V1.ShipmentFailed
        {
            EventId = Guid.NewGuid(),
            OrderId = domainEvent.OrderId.ToString(),
            Reason = domainEvent.Reason,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            Timestamp = _timeProvider.GetUtcNow()
        };
    }
}
