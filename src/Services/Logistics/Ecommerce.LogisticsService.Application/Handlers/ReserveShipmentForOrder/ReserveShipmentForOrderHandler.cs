using Ecommerce.LogisticsService.Domain;

namespace Ecommerce.LogisticsService.Application;

public sealed class ReserveShipmentForOrderHandler
{
    private readonly ILogisticsDbContext _dbContext;

    public ReserveShipmentForOrderHandler(ILogisticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShipmentDetailsDto> ExecuteAsync(ReserveShipmentForOrderCommand command, CancellationToken cancellationToken)
    {
        var shipment = Shipment.CreateForOrder(command.OrderId, command.CorrelationId, command.CausationId);
        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shipment.ToDetailsDto();
    }
}

public sealed record ReserveShipmentForOrderCommand(
    Guid OrderId,
    Guid CorrelationId,
    Guid? CausationId);
