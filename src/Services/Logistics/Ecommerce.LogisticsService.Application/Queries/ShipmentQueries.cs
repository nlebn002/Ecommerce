using Ecommerce.LogisticsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.LogisticsService.Application;

internal static class ShipmentQueries
{
    public static Task<Shipment?> GetShipmentByIdAsync(
        this ILogisticsDbContext dbContext,
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        return dbContext.Shipments
            .AsNoTracking()
            .SingleOrDefaultAsync(shipment => shipment.Id == shipmentId, cancellationToken);
    }

    public static Task<Shipment?> GetShipmentByOrderIdAsync(
        this ILogisticsDbContext dbContext,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Shipments
            .AsNoTracking()
            .SingleOrDefaultAsync(shipment => shipment.OrderId == orderId, cancellationToken);
    }
}
