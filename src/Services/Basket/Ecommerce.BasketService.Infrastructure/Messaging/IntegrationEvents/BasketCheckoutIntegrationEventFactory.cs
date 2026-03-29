using Ecommerce.BasketService.Domain;

namespace Ecommerce.BasketService.Infrastructure.Messaging.IntegrationEvents;

public sealed class BasketCheckoutIntegrationEventFactory
{
    private readonly TimeProvider _timeProvider;

    public BasketCheckoutIntegrationEventFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public BasketCheckoutIntegrationEvents Create(BasketCheckedOutDomainEvent domainEvent)
    {
        var correlationId = Guid.NewGuid();
        var timestamp = _timeProvider.GetUtcNow();

        return new BasketCheckoutIntegrationEvents(
            new Ecommerce.Contracts.V1.BasketCheckedOut
            {
                EventId = Guid.NewGuid(),
                BasketId = domainEvent.BasketId,
                CustomerId = domainEvent.CustomerId,
                Items = domainEvent.Items
                    .Select(item => new Ecommerce.Contracts.V1.BasketItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToArray(),
                ItemsTotal = domainEvent.ItemsTotal,
                CorrelationId = correlationId,
                Timestamp = timestamp
            },
            new Ecommerce.Contracts.V2.BasketCheckedOut
            {
                EventId = Guid.NewGuid(),
                BasketId = domainEvent.BasketId,
                CustomerId = domainEvent.CustomerId,
                Items = domainEvent.Items
                    .Select(item => new Ecommerce.Contracts.V2.BasketItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToArray(),
                ItemsTotal = domainEvent.ItemsTotal,
                CorrelationId = correlationId,
                Timestamp = timestamp
            });
    }
}
