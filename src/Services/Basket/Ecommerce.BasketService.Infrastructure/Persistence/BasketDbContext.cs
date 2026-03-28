using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public sealed class BasketDbContext : DbContext, IBasketDbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Basket> Baskets => Set<Basket>();

    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BasketDbContext).Assembly);
        modelBuilder.ApplyAuditPropertyConventions();
        modelBuilder.ApplySoftDeleteQueryFilters();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BasketConflictException("The basket was changed by another request. Reload and try again.");
        }
    }
}

