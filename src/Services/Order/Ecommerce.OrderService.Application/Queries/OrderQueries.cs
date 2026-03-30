using Ecommerce.OrderService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Application;

internal static class OrderQueries
{
    public static Task<Order?> GetOrderByIdAsync(
        this IOrderDbContext dbContext,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public static Task<List<Order>> GetOrdersByCustomerIdAsync(
        this IOrderDbContext dbContext,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Items)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
