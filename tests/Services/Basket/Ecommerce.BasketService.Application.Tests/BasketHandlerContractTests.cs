using Ecommerce.BasketService.Application;
using Ecommerce.BasketService.Domain;
using Ecommerce.BasketService.Infrastructure.Messaging.Outbox;
using Ecommerce.BasketService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.BasketService.Application.Tests;

public sealed class BasketHandlerContractTests
{
    [Fact]
    public async Task CreateBasketHandler_CreatesBasketExplicitly()
    {
        await using var dbContext = CreateDbContext(nameof(CreateBasketHandler_CreatesBasketExplicitly));
        var handler = new CreateBasketHandler(dbContext);
        var customerId = Guid.NewGuid();

        var basket = await handler.ExecuteAsync(new CreateBasketCommand(customerId), CancellationToken.None);

        basket.CustomerId.Should().Be(customerId);
        basket.Status.Should().Be(BasketStatus.Active.ToString());

        var persistedBasket = await dbContext.Baskets.SingleAsync();
        persistedBasket.Id.Should().Be(basket.BasketId);
        persistedBasket.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task GetBasketHandler_WhenBasketExists_ReturnsBasketWithoutMutatingStorage()
    {
        await using var dbContext = CreateDbContext(nameof(GetBasketHandler_WhenBasketExists_ReturnsBasketWithoutMutatingStorage));
        var basket = Basket.Create(Guid.NewGuid());
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new GetBasketHandler(dbContext);

        var result = await handler.ExecuteAsync(new GetBasketQuery(basket.Id), CancellationToken.None);

        result.BasketId.Should().Be(basket.Id);
        (await dbContext.Baskets.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetBasketHandler_WhenBasketIsMissing_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext(nameof(GetBasketHandler_WhenBasketIsMissing_ThrowsNotFound));
        var handler = new GetBasketHandler(dbContext);

        var act = () => handler.ExecuteAsync(new GetBasketQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<BasketNotFoundException>()
            .WithMessage("The basket was not found.");
        (await dbContext.Baskets.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AddOrUpdateBasketItemHandler_WhenBasketIsMissing_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext(nameof(AddOrUpdateBasketItemHandler_WhenBasketIsMissing_ThrowsNotFound));
        var handler = new AddOrUpdateBasketItemHandler(dbContext);

        var act = () => handler.ExecuteAsync(
            new AddOrUpdateBasketItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Keyboard", 1, 25m),
            CancellationToken.None);

        await act.Should().ThrowAsync<BasketNotFoundException>();
    }

    private static BasketDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BasketDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BasketDbContext(options, new NullDomainEventOutboxMessageFactory());
    }

    private sealed class NullDomainEventOutboxMessageFactory : IDomainEventOutboxMessageFactory
    {
        public IReadOnlyCollection<OutboxMessage> CreateMessages(IReadOnlyCollection<IDomainEvent> domainEvents) => [];
    }
}
