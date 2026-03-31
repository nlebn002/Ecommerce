using Ecommerce.LogisticsService.Application;
using MassTransit;

namespace Ecommerce.LogisticsService.Infrastructure.Messaging.Consumers;

public sealed class OrderCreatedConsumer : IConsumer<Ecommerce.Contracts.V1.OrderCreated>
{
    private readonly ReserveShipmentForOrderHandler _handler;

    public OrderCreatedConsumer(ReserveShipmentForOrderHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<Ecommerce.Contracts.V1.OrderCreated> context)
    {
        if (!Guid.TryParse(context.Message.OrderId, out var orderId))
        {
            throw new InvalidOperationException($"Order created event contains an invalid order id '{context.Message.OrderId}'.");
        }

        await _handler.ExecuteAsync(
            new ReserveShipmentForOrderCommand(orderId, context.Message.CorrelationId, context.Message.EventId),
            context.CancellationToken);
    }
}
