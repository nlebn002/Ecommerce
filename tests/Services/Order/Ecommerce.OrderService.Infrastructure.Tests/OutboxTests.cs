using System.Text.Json;
using Ecommerce.OrderService.Domain;
using Ecommerce.OrderService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.OrderService.Infrastructure.Messaging.Outbox;
using Ecommerce.OrderService.Infrastructure.Persistence;
using Ecommerce.OrderService.Infrastructure.Persistence.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.OrderService.Infrastructure.Tests;

public sealed class OutboxTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsOutboxMessagesAlongsideOrderChanges()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 15, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext(nameof(SaveChangesAsync_PersistsOutboxMessagesAlongsideOrderChanges), timeProvider);
        var order = Order.Create(
            Guid.NewGuid(),
            [new CreateOrderItemData(Guid.NewGuid(), "Keyboard", 2, 25m)],
            50m,
            Guid.NewGuid(),
            Guid.NewGuid());

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        order.DomainEvents.Should().BeEmpty();

        var outboxMessage = await dbContext.OutboxMessages.SingleAsync();
        outboxMessage.Type.Should().Be(OutboxMessageTypes.OrderCreatedV1);
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        outboxMessage.AttemptCount.Should().Be(0);
    }

    [Fact]
    public void DomainEventOutboxMessageFactory_MapsOrderEventsIntoContracts()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 15, 30, 0, TimeSpan.Zero));
        var factory = new DomainEventOutboxMessageFactory(new OrderIntegrationEventFactory(timeProvider), timeProvider);
        var domainEvent = new OrderConfirmedDomainEvent(
            Guid.NewGuid(),
            12.5m,
            62.5m,
            "Confirmed",
            Guid.NewGuid(),
            Guid.NewGuid());

        var outboxMessage = factory.CreateMessages([domainEvent]).Single();
        var integrationEvent = JsonSerializer.Deserialize<Ecommerce.Contracts.V1.OrderConfirmed>(outboxMessage.Payload);

        outboxMessage.Type.Should().Be(OutboxMessageTypes.OrderConfirmedV1);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.ShippingPrice.Should().Be(12.5m);
        integrationEvent.FinalTotal.Should().Be(62.5m);
        integrationEvent.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_PublishesAndMarksProcessed()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 30, 16, 0, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher();
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_PublishesAndMarksProcessed), timeProvider);
        dbContext.OutboxMessages.Add(OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.OrderCreatedV1, "{}", timeProvider.UtcNow.UtcDateTime));
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(1);
        publisher.PublishedMessageIds.Should().ContainSingle();
        (await dbContext.OutboxMessages.SingleAsync()).ProcessedOnUtc.Should().Be(timeProvider.UtcNow.UtcDateTime);
    }

    private static OrderDbContext CreateDbContext(string databaseName, TimeProvider timeProvider)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(new AuditSaveChangesInterceptor(timeProvider))
            .Options;

        return new OrderDbContext(options, new DomainEventOutboxMessageFactory(new OrderIntegrationEventFactory(timeProvider), timeProvider));
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
