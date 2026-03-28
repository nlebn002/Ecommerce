using Ecommerce.Basket.Domain;

namespace Ecommerce.Basket.Application;

public interface IBasketDbContext
{
    DbSet<Basket> Baskets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
