using Ecommerce.LogisticsService.Domain;
using Microsoft.EntityFrameworkCore;

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
        var existingShipment = await _dbContext.GetShipmentByOrderIdAsync(command.OrderId, cancellationToken);
        if (existingShipment is not null)
        {
            return existingShipment.ToDetailsDto();
        }

        var shipment = Shipment.CreateForOrder(command.OrderId, command.CorrelationId, command.CausationId);
        _dbContext.Shipments.Add(shipment);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            existingShipment = await _dbContext.GetShipmentByOrderIdAsync(command.OrderId, cancellationToken);
            if (existingShipment is not null)
            {
                return existingShipment.ToDetailsDto();
            }

            throw;
        }

        return shipment.ToDetailsDto();
    }
}

public sealed record ReserveShipmentForOrderCommand(
    Guid OrderId,
    Guid CorrelationId,
    Guid? CausationId);
