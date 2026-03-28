using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using MassTransit;

namespace Ecommerce.BasketService.Infrastructure.Messaging;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public DomainEventDispatcher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case BasketCheckedOutDomainEvent basketCheckedOut:
                    await PublishBasketCheckedOutAsync(basketCheckedOut, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"No dispatcher is registered for domain event '{domainEvent.GetType().Name}'.");
            }
        }
    }

    private async Task PublishBasketCheckedOutAsync(
        BasketCheckedOutDomainEvent basketCheckedOut,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var itemsV1 = basketCheckedOut.Items
            .Select(item => new Ecommerce.Contracts.V1.BasketItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            })
            .ToArray();

        var itemsV2 = basketCheckedOut.Items
            .Select(item => new Ecommerce.Contracts.V2.BasketItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            })
            .ToArray();

        await _publishEndpoint.Publish(new Ecommerce.Contracts.V1.BasketCheckedOut
        {
            BasketId = basketCheckedOut.BasketId,
            CustomerId = basketCheckedOut.CustomerId,
            Items = itemsV1,
            ItemsTotal = basketCheckedOut.ItemsTotal,
            CorrelationId = correlationId
        }, cancellationToken);

        await _publishEndpoint.Publish(new Ecommerce.Contracts.V2.BasketCheckedOut
        {
            BasketId = basketCheckedOut.BasketId,
            CustomerId = basketCheckedOut.CustomerId,
            Items = itemsV2,
            ItemsTotal = basketCheckedOut.ItemsTotal,
            CorrelationId = correlationId
        }, cancellationToken);
    }
}
