using Ecommerce.OrderService.Application;
using MassTransit;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Consumers;

public sealed class BasketCheckedOutConsumer :
    IConsumer<Ecommerce.Contracts.V1.BasketCheckedOut>,
    IConsumer<Ecommerce.Contracts.V2.BasketCheckedOut>
{
    private readonly CreateOrderFromBasketCheckoutHandler _handler;

    public BasketCheckedOutConsumer(CreateOrderFromBasketCheckoutHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<Ecommerce.Contracts.V1.BasketCheckedOut> context)
    {
        await _handler.ExecuteAsync(
            new CreateOrderFromBasketCheckoutCommand(
                context.Message.CustomerId,
                context.Message.Items
                    .Select(item => new CreateOrderLineItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                    .ToArray(),
                context.Message.ItemsTotal,
                context.Message.CorrelationId,
                context.Message.EventId),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<Ecommerce.Contracts.V2.BasketCheckedOut> context)
    {
        await _handler.ExecuteAsync(
            new CreateOrderFromBasketCheckoutCommand(
                context.Message.CustomerId,
                context.Message.Items
                    .Select(item => new CreateOrderLineItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                    .ToArray(),
                context.Message.ItemsTotal,
                context.Message.CorrelationId,
                context.Message.EventId),
            context.CancellationToken);
    }
}
