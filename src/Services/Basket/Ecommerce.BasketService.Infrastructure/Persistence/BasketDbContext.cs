using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Messaging.Outbox;
using Ecommerce.BasketService.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public sealed class BasketDbContext : DbContext, IBasketDbContext
{
    private readonly IDomainEventOutboxMessageFactory _domainEventOutboxMessageFactory;

    public BasketDbContext(
        DbContextOptions<BasketDbContext> options,
        IDomainEventOutboxMessageFactory domainEventOutboxMessageFactory)
        : base(options)
    {
        _domainEventOutboxMessageFactory = domainEventOutboxMessageFactory;
    }

    public DbSet<Basket> Baskets => Set<Basket>();

    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
            var domainEvents = entitiesWithDomainEvents
                .SelectMany(entity => entity.DomainEvents)
                .ToArray();

            if (domainEvents.Length > 0)
            {
                var outboxMessages = _domainEventOutboxMessageFactory.CreateMessages(domainEvents);
                OutboxMessages.AddRange(outboxMessages);
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var entity in entitiesWithDomainEvents)
            {
                entity.ClearDomainEvents();
            }

            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw BasketException.Conflict(BasketErrorCode.ConcurrencyConflict, "The basket was changed by another request. Reload and try again.");
        }
    }
}

