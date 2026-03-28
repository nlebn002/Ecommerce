using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
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

        return new BasketDbContext(optionsBuilder.Options, NoOpDomainEventDispatcher.Instance);
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public static NoOpDomainEventDispatcher Instance { get; } = new();

        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
