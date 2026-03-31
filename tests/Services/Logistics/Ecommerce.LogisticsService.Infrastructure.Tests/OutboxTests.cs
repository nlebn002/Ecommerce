using System.Text.Json;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using Ecommerce.LogisticsService.Domain;
using Ecommerce.LogisticsService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.LogisticsService.Infrastructure.Messaging.Outbox;
using Ecommerce.LogisticsService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.LogisticsService.Infrastructure.Tests;

public sealed class OutboxTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsOutboxMessagesAlongsideShipmentChanges()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 15, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext(nameof(SaveChangesAsync_PersistsOutboxMessagesAlongsideShipmentChanges), timeProvider);
        var shipment = Shipment.CreateForOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync();

        shipment.DomainEvents.Should().BeEmpty();

        var outboxTypes = await dbContext.OutboxMessages.Select(message => message.Type).OrderBy(type => type).ToArrayAsync();
        outboxTypes.Should().BeEquivalentTo([OutboxMessageTypes.ShipmentRepricedV1, OutboxMessageTypes.ShipmentReservedV1]);
    }

    [Fact]
    public void DomainEventOutboxMessageFactory_MapsShipmentEventsIntoContracts()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 15, 30, 0, TimeSpan.Zero));
        var factory = new DomainEventOutboxMessageFactory(new LogisticsIntegrationEventFactory(timeProvider), timeProvider);
        var domainEvent = new ShipmentRepricedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 12.5m, Guid.NewGuid(), Guid.NewGuid());

        var outboxMessage = factory.CreateMessages([domainEvent]).Single();
        var integrationEvent = JsonSerializer.Deserialize<Ecommerce.Contracts.V1.ShipmentRepriced>(outboxMessage.Payload);

        outboxMessage.Type.Should().Be(OutboxMessageTypes.ShipmentRepricedV1);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.ShippingPrice.Should().Be(12.5m);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_PublishesAndMarksProcessed()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 16, 0, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher();
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_PublishesAndMarksProcessed), timeProvider);
        dbContext.OutboxMessages.Add(OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.ShipmentReservedV1, "{}", timeProvider.UtcNow.UtcDateTime));
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(1);
        publisher.PublishedMessageIds.Should().ContainSingle();
        (await dbContext.OutboxMessages.SingleAsync()).ProcessedOnUtc.Should().Be(timeProvider.UtcNow.UtcDateTime);
    }

    private static LogisticsDbContext CreateDbContext(string databaseName, TimeProvider timeProvider)
    {
        var options = new DbContextOptionsBuilder<LogisticsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(new AuditSaveChangesInterceptor(timeProvider))
            .Options;

        return new LogisticsDbContext(options, new DomainEventOutboxMessageFactory(new LogisticsIntegrationEventFactory(timeProvider), timeProvider));
    }

    private sealed class FakeOutboxMessagePublisher : IOutboxMessagePublisher
    {
        public List<Guid> PublishedMessageIds { get; } = [];

        public Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            PublishedMessageIds.Add(outboxMessage.Id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
