using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed class GetOrderHandler
{
    private readonly IOrderDbContext _dbContext;

    public GetOrderHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto> ExecuteAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await _dbContext.GetOrderByIdAsync(query.OrderId, cancellationToken);
        if (order is null)
        {
            throw OrderException.NotFound(OrderErrorCode.OrderNotFound, "The order was not found.");
        }

        return order.ToDetailsDto();
    }
}

public sealed record GetOrderQuery(Guid OrderId);
