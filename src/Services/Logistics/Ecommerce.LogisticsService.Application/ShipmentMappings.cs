using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Application;

internal static class ShipmentMappings
{
    public static ShipmentDetailsDto ToDetailsDto(this Shipment shipment)
    {
        return new ShipmentDetailsDto(
            shipment.Id,
            shipment.OrderId,
            shipment.Carrier,
            shipment.ShippingPrice,
            shipment.Status.ToString(),
            shipment.FailureReason);
    }

    public static ShipmentSummaryDto ToSummaryDto(this Shipment shipment)
    {
        return new ShipmentSummaryDto(
            shipment.Id,
            shipment.OrderId,
            shipment.Carrier,
            shipment.ShippingPrice,
            shipment.Status.ToString());
    }
}
