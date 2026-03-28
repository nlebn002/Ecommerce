using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using MassTransit;

namespace Ecommerce.BasketService.Infrastructure;

public sealed class BasketCheckoutPublisher : IBasketCheckoutPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public BasketCheckoutPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync(Basket basket, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var itemsV1 = basket.Items
            .Select(item => new Ecommerce.Contracts.V1.BasketItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            })
            .ToArray();

        var itemsV2 = basket.Items
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
            BasketId = basket.Id,
            CustomerId = basket.CustomerId,
            Items = itemsV1,
            ItemsTotal = basket.Total,
            CorrelationId = correlationId
        }, cancellationToken);

        await _publishEndpoint.Publish(new Ecommerce.Contracts.V2.BasketCheckedOut
        {
            BasketId = basket.Id,
            CustomerId = basket.CustomerId,
            Items = itemsV2,
            ItemsTotal = basket.Total,
            CorrelationId = correlationId
        }, cancellationToken);
    }
}

