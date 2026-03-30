using Ecommerce.OrderService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Application;

public interface IOrderDbContext
{
    DbSet<Order> Orders { get; }

    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
