using Ecommerce.Basket.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Basket.Application;

internal static class BasketQueries
{
    public static Task<Basket?> GetBasketByIdAsync(
        this IBasketDbContext dbContext,
        string basketId,
        CancellationToken cancellationToken)
    {
        return dbContext.Baskets
            .Include(basket => basket.Items)
            .SingleOrDefaultAsync(basket => basket.Id == basketId, cancellationToken);
    }
}
