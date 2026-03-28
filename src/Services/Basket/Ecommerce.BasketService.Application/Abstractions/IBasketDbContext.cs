using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Application;

public interface IBasketDbContext
{
    DbSet<Basket> Baskets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

