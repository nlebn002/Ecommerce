using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public static class BasketDevDataSeeder
{
    private static readonly Guid BasketId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid CoffeeBeansProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MugProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task<BasketDevDataSeedResult> SeedAsync(
        IServiceProvider services,
        ILogger logger,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Dev seed data can only be applied when ASPNETCORE_ENVIRONMENT is Development.");
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        logger.LogInformation("Applying Basket database migrations before explicit dev seeding.");
        await dbContext.Database.MigrateAsync(cancellationToken);

        var existingBasket = await dbContext.Baskets
            .Include(basket => basket.Items)
            .SingleOrDefaultAsync(basket => basket.Id == BasketId, cancellationToken);

        if (existingBasket is not null)
        {
            logger.LogInformation("Basket dev seed data already exists. No changes applied.");
            return new BasketDevDataSeedResult(false, "Basket dev seed data already exists. No changes applied.");
        }

        var basket = Basket.Create(BasketId);
        basket.AddOrUpdateItem(CoffeeBeansProductId, "Coffee Beans", 2, 12.50m);
        basket.AddOrUpdateItem(MugProductId, "Ceramic Mug", 1, 8.00m);

        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Basket dev seed data applied successfully.");
        return new BasketDevDataSeedResult(true, "Basket dev seed data applied successfully.");
    }
}

public sealed record BasketDevDataSeedResult(bool Applied, string Message);
