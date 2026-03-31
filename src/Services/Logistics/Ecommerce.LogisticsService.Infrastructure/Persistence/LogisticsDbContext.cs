using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using Ecommerce.LogisticsService.Application;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using DomainEntity = Ecommerce.LogisticsService.Domain.Entity;

namespace Ecommerce.LogisticsService.Infrastructure.Persistence;

public sealed class LogisticsDbContext : DbContext, ILogisticsDbContext, IOutboxDbContext
{
    private readonly IDomainEventOutboxMessageFactory _domainEventOutboxMessageFactory;

    public LogisticsDbContext(
        DbContextOptions<LogisticsDbContext> options,
        IDomainEventOutboxMessageFactory domainEventOutboxMessageFactory)
        : base(options)
    {
        _domainEventOutboxMessageFactory = domainEventOutboxMessageFactory;
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogisticsDbContext).Assembly);
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
            throw LogisticsException.Conflict(
                LogisticsErrorCode.ConcurrencyConflict,
                "The shipment was changed by another request. Reload and try again.");
        }
    }
}
