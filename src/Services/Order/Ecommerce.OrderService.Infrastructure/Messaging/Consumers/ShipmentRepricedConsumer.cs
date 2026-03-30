using Ecommerce.OrderService.Application;
using MassTransit;

namespace Ecommerce.OrderService.Infrastructure.Messaging.Consumers;

public sealed class ShipmentRepricedConsumer : IConsumer<Ecommerce.Contracts.V1.ShipmentRepriced>
{
    private readonly ConfirmOrderHandler _handler;

    public ShipmentRepricedConsumer(ConfirmOrderHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<Ecommerce.Contracts.V1.ShipmentRepriced> context)
    {
        if (!Guid.TryParse(context.Message.OrderId, out var orderId))
        {
            throw new InvalidOperationException($"Shipment repriced event contains an invalid order id '{context.Message.OrderId}'.");
        }

        await _handler.ExecuteAsync(
            new ConfirmOrderCommand(orderId, context.Message.ShippingPrice, context.Message.CorrelationId, context.Message.EventId),
            context.CancellationToken);
    }
}
