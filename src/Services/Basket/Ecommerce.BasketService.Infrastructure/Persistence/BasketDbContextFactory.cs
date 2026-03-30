using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Messaging.Outbox;
using Ecommerce.Common.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.BasketService.Infrastructure.Persistence;

public sealed class BasketDbContextFactory : IDesignTimeDbContextFactory<BasketDbContext>
{
    public BasketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BasketDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BasketDb")
            ?? "Host=localhost;Port=5432;Database=basketdb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new BasketDbContext(optionsBuilder.Options, NoOpDomainEventOutboxMessageFactory.Instance);
    }

    private sealed class NoOpDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public static NoOpDomainEventOutboxMessageFactory Instance { get; } = new();

        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            return [];
        }
    }
}
