using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.OrderService.Infrastructure.Persistence;

public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__OrderDb")
            ?? "Host=localhost;Port=5432;Database=orderdb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new OrderDbContext(optionsBuilder.Options, NoOpDomainEventOutboxMessageFactory.Instance);
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
