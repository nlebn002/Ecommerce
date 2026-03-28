using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public sealed class BasketDbContext : DbContext, IBasketDbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public BasketDbContext(DbContextOptions<BasketDbContext> options, IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
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
        var entitiesWithDomainEvents = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            var domainEvents = entitiesWithDomainEvents
                .SelectMany(entity => entity.DomainEvents)
                .ToArray();

            if (domainEvents.Length > 0)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

                foreach (var entity in entitiesWithDomainEvents)
                {
                    entity.ClearDomainEvents();
                }
            }

            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BasketConflictException("The basket was changed by another request. Reload and try again.");
        }
    }
}

