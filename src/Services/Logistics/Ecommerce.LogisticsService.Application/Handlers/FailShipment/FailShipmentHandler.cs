using Ecommerce.LogisticsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.LogisticsService.Application;

public sealed class FailShipmentHandler
{
    private readonly ILogisticsDbContext _dbContext;

    public FailShipmentHandler(ILogisticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShipmentDetailsDto> ExecuteAsync(FailShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments.SingleOrDefaultAsync(
            entity => entity.Id == command.ShipmentId,
            cancellationToken);

        if (shipment is null)
        {
            throw LogisticsException.NotFound(LogisticsErrorCode.ShipmentNotFound, "The shipment was not found.");
        }

        shipment.Fail(command.Reason, command.CorrelationId, command.CausationId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shipment.ToDetailsDto();
    }
}

public sealed record FailShipmentCommand(
    Guid ShipmentId,
    string Reason,
    Guid CorrelationId,
    Guid? CausationId);
