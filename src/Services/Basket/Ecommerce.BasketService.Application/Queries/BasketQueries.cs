using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Application;

internal static class BasketQueries
{
    public static Task<Basket?> GetBasketByIdAsync(
        this IBasketDbContext dbContext,
        Guid basketId,
        CancellationToken cancellationToken)
    {
        return dbContext.Baskets
            .Include(basket => basket.Items)
            .SingleOrDefaultAsync(basket => basket.Id == basketId, cancellationToken);
    }
}

