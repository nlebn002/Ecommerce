using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed class ConfirmOrderHandler
{
    private readonly IOrderDbContext _dbContext;

    public ConfirmOrderHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto> ExecuteAsync(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _dbContext.GetOrderByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            throw OrderException.NotFound(OrderErrorCode.OrderNotFound, "The order was not found.");
        }

        if (order.Status == OrderStatus.Confirmed)
        {
            if (order.ShippingPrice == command.ShippingPrice)
            {
                return order.ToDetailsDto();
            }

            throw OrderException.Conflict(
                OrderErrorCode.InvalidOrderState,
                "The order is already confirmed with a different shipping price.");
        }

        order.Confirm(command.ShippingPrice, command.CorrelationId, command.CausationId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.ToDetailsDto();
    }
}

public sealed record ConfirmOrderCommand(
    Guid OrderId,
    decimal ShippingPrice,
    Guid CorrelationId,
    Guid? CausationId);
