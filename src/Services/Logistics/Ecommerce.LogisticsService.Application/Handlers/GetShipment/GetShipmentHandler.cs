using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Application;

public sealed class GetShipmentHandler
{
    private readonly ILogisticsDbContext _dbContext;

    public GetShipmentHandler(ILogisticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShipmentDetailsDto> ExecuteAsync(GetShipmentQuery query, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.GetShipmentByIdAsync(query.ShipmentId, cancellationToken);
        if (shipment is null)
        {
            throw LogisticsException.NotFound(LogisticsErrorCode.ShipmentNotFound, "The shipment was not found.");
        }

        return shipment.ToDetailsDto();
    }
}

public sealed record GetShipmentQuery(Guid ShipmentId);
