namespace Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;

public static class OutboxMessageTypes
{
    public const string ShipmentReservedV1 = "shipment-reserved.v1";
    public const string ShipmentRepricedV1 = "shipment-repriced.v1";
    public const string ShipmentFailedV1 = "shipment-failed.v1";
}
