namespace Ecommerce.OrderService.Infrastructure.Messaging.Outbox;

public static class OutboxMessageTypes
{
    public const string OrderCreatedV1 = "order-created.v1";
    public const string OrderConfirmedV1 = "order-confirmed.v1";
    public const string OrderCancelledV1 = "order-cancelled.v1";
}
