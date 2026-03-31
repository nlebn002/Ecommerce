using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Application;

public sealed class GetShipmentByOrderHandler
{
    private readonly ILogisticsDbContext _dbContext;

    public GetShipmentByOrderHandler(ILogisticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShipmentSummaryDto> ExecuteAsync(GetShipmentByOrderQuery query, CancellationToken cancellationToken)
    {
        if (query.OrderId == Guid.Empty)
        {
            throw LogisticsException.Validation(LogisticsErrorCode.InvalidOrderId, "orderId", "Order id is required.");
        }

        var shipment = await _dbContext.GetShipmentByOrderIdAsync(query.OrderId, cancellationToken);
        if (shipment is null)
        {
            throw LogisticsException.NotFound(LogisticsErrorCode.ShipmentNotFound, "The shipment was not found.");
        }

        return shipment.ToSummaryDto();
    }
}

public sealed record GetShipmentByOrderQuery(Guid OrderId);
