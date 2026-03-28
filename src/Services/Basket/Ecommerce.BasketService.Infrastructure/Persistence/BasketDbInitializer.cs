using Ecommerce.BasketService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public static class BasketDbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider services, IHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        await dbContext.Database.MigrateAsync();
        await SeedAsync(dbContext, environment);
    }

    private static async Task SeedAsync(BasketDbContext dbContext, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        if (await dbContext.Baskets.AnyAsync())
        {
            return;
        }

        var basket = Basket.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"), "seed-customer");
        basket.AddOrUpdateItem("prod-coffee", "Coffee Beans", 2, 12.50m);
        basket.AddOrUpdateItem("prod-mug", "Ceramic Mug", 1, 8.00m);

        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();
    }
}
