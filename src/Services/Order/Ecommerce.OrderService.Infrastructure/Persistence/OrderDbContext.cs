using Ecommerce.OrderService.Application;
using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainEntity = Ecommerce.OrderService.Domain.Entity;

namespace Ecommerce.OrderService.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext, IOrderDbContext, IOutboxDbContext
{
    private readonly IDomainEventOutboxMessageFactory _domainEventOutboxMessageFactory;

    public OrderDbContext(
        DbContextOptions<OrderDbContext> options,
        IDomainEventOutboxMessageFactory domainEventOutboxMessageFactory)
        : base(options)
    {
        _domainEventOutboxMessageFactory = domainEventOutboxMessageFactory;
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        modelBuilder.ApplyAuditPropertyConventions();
        modelBuilder.ApplySoftDeleteQueryFilters();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithDomainEvents = ChangeTracker.Entries<DomainEntity>()
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
            throw OrderException.Conflict(OrderErrorCode.ConcurrencyConflict, "The order was changed by another request. Reload and try again.");
        }
    }
}
