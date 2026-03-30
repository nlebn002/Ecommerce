using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed class GetCustomerOrdersHandler
{
    private readonly IOrderDbContext _dbContext;

    public GetCustomerOrdersHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> ExecuteAsync(GetCustomerOrdersQuery query, CancellationToken cancellationToken)
    {
        if (query.CustomerId == Guid.Empty)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidCustomerId, "customerId", "Customer id is required.");
        }

        var orders = await _dbContext.GetOrdersByCustomerIdAsync(query.CustomerId, cancellationToken);
        return orders.Select(order => order.ToSummaryDto()).ToArray();
    }
}

public sealed record GetCustomerOrdersQuery(Guid CustomerId);
