using System.Text.Json;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Messaging.IntegrationEvents;
using Ecommerce.BasketService.Infrastructure.Messaging.Outbox;
using Ecommerce.BasketService.Infrastructure.Persistence;
using Ecommerce.Common.Messaging.Outbox;
using Ecommerce.Common.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ecommerce.BasketService.Infrastructure.Tests;

public sealed class OutboxTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsOutboxMessagesAlongsideBasketChanges()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 10, 0, 0, TimeSpan.Zero));
        await using var dbContext = CreateDbContext(nameof(SaveChangesAsync_PersistsOutboxMessagesAlongsideBasketChanges), timeProvider);
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 2, 25.50m);
        basket.Checkout();

        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();

        basket.DomainEvents.Should().BeEmpty();

        var persistedBasket = await dbContext.Baskets.SingleAsync();
        persistedBasket.Status.Should().Be(BasketStatus.CheckedOut);

        var outboxMessages = await dbContext.OutboxMessages
            .OrderBy(message => message.Type)
            .ToListAsync();

        outboxMessages.Should().HaveCount(2);
        outboxMessages.Should().OnlyContain(message =>
            message.DiscardedOnUtc == null &&
            message.ProcessedOnUtc == null &&
            message.AttemptCount == 0 &&
            message.OccurredOnUtc == timeProvider.UtcNow.UtcDateTime);
    }

    [Fact]
    public void DomainEventOutboxMessageFactory_MapsCheckoutIntoVersionedContracts()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 10, 15, 0, TimeSpan.Zero));
        var factory = new DomainEventOutboxMessageFactory(
            new BasketCheckoutIntegrationEventFactory(timeProvider),
            timeProvider);
        var domainEvent = new BasketCheckedOutDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new BasketCheckedOutItem(Guid.NewGuid(), "Keyboard", 3, 25m)],
            75m);

        var outboxMessages = factory.CreateMessages([domainEvent]).OrderBy(message => message.Type).ToArray();

        outboxMessages.Should().HaveCount(2);
        outboxMessages.Select(message => message.Type).Should().ContainInOrder(
            OutboxMessageTypes.BasketCheckedOutV1,
            OutboxMessageTypes.BasketCheckedOutV2);

        var v1 = JsonSerializer.Deserialize<Ecommerce.Contracts.V1.BasketCheckedOut>(outboxMessages[0].Payload);
        var v2 = JsonSerializer.Deserialize<Ecommerce.Contracts.V2.BasketCheckedOut>(outboxMessages[1].Payload);

        v1.Should().NotBeNull();
        v2.Should().NotBeNull();
        v1!.CorrelationId.Should().Be(v2!.CorrelationId);
        v1.ItemsTotal.Should().Be(75m);
        v2.Items.Should().ContainSingle(item => item.ProductName == "Keyboard" && item.Quantity == 3);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_PublishesAndMarksMessagesProcessed()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 11, 0, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher();
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_PublishesAndMarksMessagesProcessed), timeProvider);
        dbContext.OutboxMessages.Add(OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.BasketCheckedOutV1, "{}", timeProvider.UtcNow.UtcDateTime));
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, CreateOptions(), timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(1);
        publisher.PublishedMessageIds.Should().ContainSingle();

        var persistedMessage = await dbContext.OutboxMessages.SingleAsync();
        persistedMessage.ProcessedOnUtc.Should().Be(timeProvider.UtcNow.UtcDateTime);
        persistedMessage.DiscardedOnUtc.Should().BeNull();
        persistedMessage.AttemptCount.Should().Be(0);
        persistedMessage.LastError.Should().BeNull();
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPublishFailsBelowMaxRetry_KeepsMessagePending()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 11, 30, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher(shouldThrow: true);
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_WhenPublishFailsBelowMaxRetry_KeepsMessagePending), timeProvider);
        dbContext.OutboxMessages.Add(OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.BasketCheckedOutV1, "{}", timeProvider.UtcNow.UtcDateTime));
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, CreateOptions(), timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(0);

        var persistedMessage = await dbContext.OutboxMessages.SingleAsync();
        persistedMessage.ProcessedOnUtc.Should().BeNull();
        persistedMessage.DiscardedOnUtc.Should().BeNull();
        persistedMessage.AttemptCount.Should().Be(1);
        persistedMessage.LastError.Should().Contain("Simulated broker failure");
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WhenPublishFailsAtMaxRetry_DiscardsMessage()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 11, 45, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher(shouldThrow: true);
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_WhenPublishFailsAtMaxRetry_DiscardsMessage), timeProvider);
        var message = OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.BasketCheckedOutV1, "{}", timeProvider.UtcNow.UtcDateTime);
        message.MarkFailed("previous failure", 3, timeProvider.UtcNow.UtcDateTime.AddMinutes(-1));
        message.MarkFailed("previous failure", 3, timeProvider.UtcNow.UtcDateTime.AddMinutes(-1));
        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, CreateOptions(), timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(0);

        var persistedMessage = await dbContext.OutboxMessages.SingleAsync();
        persistedMessage.ProcessedOnUtc.Should().BeNull();
        persistedMessage.DiscardedOnUtc.Should().Be(timeProvider.UtcNow.UtcDateTime);
        persistedMessage.AttemptCount.Should().Be(3);
        persistedMessage.LastError.Should().Contain("Simulated broker failure");
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_SkipsDiscardedMessages()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 12, 0, 0, TimeSpan.Zero));
        var publisher = new FakeOutboxMessagePublisher();
        await using var dbContext = CreateDbContext(nameof(ProcessPendingMessagesAsync_SkipsDiscardedMessages), timeProvider);
        var discardedMessage = OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.BasketCheckedOutV1, "{}", timeProvider.UtcNow.UtcDateTime.AddMinutes(-5));
        discardedMessage.MarkFailed("failure one", 3, timeProvider.UtcNow.UtcDateTime.AddMinutes(-4));
        discardedMessage.MarkFailed("failure two", 3, timeProvider.UtcNow.UtcDateTime.AddMinutes(-3));
        discardedMessage.MarkFailed("failure three", 3, timeProvider.UtcNow.UtcDateTime.AddMinutes(-2));
        var pendingMessage = OutboxMessage.Create(Guid.NewGuid(), OutboxMessageTypes.BasketCheckedOutV1, "{}", timeProvider.UtcNow.UtcDateTime);
        dbContext.OutboxMessages.AddRange(discardedMessage, pendingMessage);
        await dbContext.SaveChangesAsync();
        var processor = new OutboxMessageProcessor(dbContext, publisher, CreateOptions(), timeProvider);

        var processedCount = await processor.ProcessPendingMessagesAsync(10, CancellationToken.None);

        processedCount.Should().Be(1);
        publisher.PublishedMessageIds.Should().ContainSingle().Which.Should().Be(pendingMessage.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenBasketItemIsRemovedThroughAggregate_PersistsSoftDeletedRow_AndQueryFilterHidesIt()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 12, 0, 0, TimeSpan.Zero));
        var databaseName = nameof(SaveChangesAsync_WhenBasketItemIsRemovedThroughAggregate_PersistsSoftDeletedRow_AndQueryFilterHidesIt);

        await using (var arrangeContext = CreateDbContext(databaseName, timeProvider))
        {
            var productId = Guid.NewGuid();
            var basket = Basket.Create(Guid.NewGuid());
            basket.AddOrUpdateItem(productId, "Keyboard", 1, 25m);
            arrangeContext.Baskets.Add(basket);
            await arrangeContext.SaveChangesAsync();

            basket.RemoveItem(productId);
            await arrangeContext.SaveChangesAsync();
        }

        await using var assertContext = CreateDbContext(databaseName, timeProvider);
        (await assertContext.BasketItems.CountAsync()).Should().Be(0);

        var persistedItem = await assertContext.BasketItems
            .IgnoreQueryFilters()
            .SingleAsync();

        persistedItem.IsDeleted.Should().BeTrue();
        persistedItem.UpdatedDate.Should().Be(timeProvider.UtcNow.UtcDateTime);

        var persistedBasket = await assertContext.Baskets.SingleAsync();
        persistedBasket.Total.Should().Be(0m);

        var activeItemCount = await assertContext.BasketItems
            .Where(item => item.BasketId == persistedBasket.Id)
            .CountAsync();

        activeItemCount.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenBasketItemIsDeletedViaDbSet_RemoveIsConvertedToSoftDelete()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 29, 12, 30, 0, TimeSpan.Zero));
        var databaseName = nameof(SaveChangesAsync_WhenBasketItemIsDeletedViaDbSet_RemoveIsConvertedToSoftDelete);

        Guid itemId;

        await using (var arrangeContext = CreateDbContext(databaseName, timeProvider))
        {
            var basket = Basket.Create(Guid.NewGuid());
            basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, 25m);
            arrangeContext.Baskets.Add(basket);
            await arrangeContext.SaveChangesAsync();
            itemId = basket.Items.Single().Id;
        }

        await using (var deleteContext = CreateDbContext(databaseName, timeProvider))
        {
            var item = await deleteContext.BasketItems.IgnoreQueryFilters().SingleAsync(entity => entity.Id == itemId);
            deleteContext.BasketItems.Remove(item);
            await deleteContext.SaveChangesAsync();
        }

        await using var assertContext = CreateDbContext(databaseName, timeProvider);
        (await assertContext.BasketItems.CountAsync()).Should().Be(0);

        var persistedItem = await assertContext.BasketItems
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.Id == itemId);

        persistedItem.IsDeleted.Should().BeTrue();
        persistedItem.UpdatedDate.Should().Be(timeProvider.UtcNow.UtcDateTime);
    }

    private static BasketDbContext CreateDbContext(string databaseName, TimeProvider timeProvider)
    {
        var options = new DbContextOptionsBuilder<BasketDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(new AuditSaveChangesInterceptor(timeProvider))
            .Options;

        var outboxFactory = new DomainEventOutboxMessageFactory(
            new BasketCheckoutIntegrationEventFactory(timeProvider),
            timeProvider);

        return new BasketDbContext(options, outboxFactory);
    }

    private static IOptions<OutboxProcessorOptions> CreateOptions(int maxRetryAttempts = 3)
    {
        return Options.Create(new OutboxProcessorOptions
        {
            BatchSize = 20,
            PollInterval = TimeSpan.FromSeconds(5),
            MaxRetryAttempts = maxRetryAttempts
        });
    }

    private sealed class FakeOutboxMessagePublisher : IOutboxMessagePublisher
    {
        private readonly bool _shouldThrow;

        public FakeOutboxMessagePublisher(bool shouldThrow = false)
        {
            _shouldThrow = shouldThrow;
        }

        public List<Guid> PublishedMessageIds { get; } = [];

        public Task PublishAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            if (_shouldThrow)
            {
                throw new InvalidOperationException("Simulated broker failure");
            }

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

        public DateTimeOffset UtcNow { get; private set; }

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
