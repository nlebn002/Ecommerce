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
            .SingleOrDefaultAsync(
                basket => basket.Id == basketId,
                cancellationToken);
    }

    public static Task<Basket?> GetBasketAggregateByIdAsync(
        this IBasketDbContext dbContext,
        Guid basketId,
        CancellationToken cancellationToken)
    {
        return dbContext.Baskets
            .IgnoreQueryFilters()
            .Include(basket => basket.Items)
            .SingleOrDefaultAsync(
                basket => basket.Id == basketId && !basket.IsDeleted,
                cancellationToken);
    }
}

