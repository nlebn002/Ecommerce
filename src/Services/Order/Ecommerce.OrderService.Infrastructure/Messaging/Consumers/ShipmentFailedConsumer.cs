using Ecommerce.OrderService.Application;
using MassTransit;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Consumers;

public sealed class ShipmentFailedConsumer : IConsumer<Ecommerce.Contracts.V1.ShipmentFailed>
{
    private readonly CancelOrderHandler _handler;

    public ShipmentFailedConsumer(CancelOrderHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<Ecommerce.Contracts.V1.ShipmentFailed> context)
    {
        if (!Guid.TryParse(context.Message.OrderId, out var orderId))
        {
            throw new InvalidOperationException($"Shipment failed event contains an invalid order id '{context.Message.OrderId}'.");
        }

        await _handler.ExecuteAsync(
            new CancelOrderCommand(orderId, context.Message.Reason, context.Message.CorrelationId, context.Message.EventId),
            context.CancellationToken);
    }
}
