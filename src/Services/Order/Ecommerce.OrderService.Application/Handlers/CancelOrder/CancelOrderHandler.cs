using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed class CancelOrderHandler
{
    private readonly IOrderDbContext _dbContext;

    public CancelOrderHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto> ExecuteAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _dbContext.GetOrderByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            throw OrderException.NotFound(OrderErrorCode.OrderNotFound, "The order was not found.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            if (string.Equals(order.CancellationReason, command.Reason, StringComparison.Ordinal))
            {
                return order.ToDetailsDto();
            }

            throw OrderException.Conflict(
                OrderErrorCode.InvalidOrderState,
                "The order is already cancelled with a different reason.");
        }

        order.Cancel(command.Reason, command.CorrelationId, command.CausationId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.ToDetailsDto();
    }
}

public sealed record CancelOrderCommand(
    Guid OrderId,
    string Reason,
    Guid CorrelationId,
    Guid? CausationId);
