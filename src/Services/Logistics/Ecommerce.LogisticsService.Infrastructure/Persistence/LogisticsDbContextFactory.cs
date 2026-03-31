using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.LogisticsService.Infrastructure.Persistence;

public sealed class LogisticsDbContextFactory : IDesignTimeDbContextFactory<LogisticsDbContext>
{
    public LogisticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LogisticsDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__LogisticsDb")
            ?? "Host=localhost;Port=5432;Database=logisticsdb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new LogisticsDbContext(optionsBuilder.Options, NoOpDomainEventOutboxMessageFactory.Instance);
    }

    private sealed class NoOpDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public static NoOpDomainEventOutboxMessageFactory Instance { get; } = new();

        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents) => [];
    }
}
