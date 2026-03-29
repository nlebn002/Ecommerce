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

    [Fact]
    public async Task AddOrUpdateBasketItemHandler_WhenQuantityIsInvalid_ThrowsDomainException()
    {
        await using var dbContext = CreateDbContext(nameof(AddOrUpdateBasketItemHandler_WhenQuantityIsInvalid_ThrowsDomainException));
        var basket = Basket.Create(Guid.NewGuid());
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new AddOrUpdateBasketItemHandler(dbContext);

        var act = () => handler.ExecuteAsync(
            new AddOrUpdateBasketItemCommand(basket.Id, Guid.NewGuid(), "Keyboard", 0, 25m),
            CancellationToken.None);

        await act.Should().ThrowAsync<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "invalid_quantity" && exception.Field == "quantity");
    }

    [Fact]
    public async Task CheckoutBasketHandler_WhenBasketIsEmpty_ThrowsDomainException()
    {
        await using var dbContext = CreateDbContext(nameof(CheckoutBasketHandler_WhenBasketIsEmpty_ThrowsDomainException));
        var basket = Basket.Create(Guid.NewGuid());
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new CheckoutBasketHandler(dbContext);

        var act = () => handler.ExecuteAsync(new CheckoutBasketCommand(basket.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_empty" && exception.Field == "basketId");
    }

    [Fact]
    public async Task RemoveBasketItemHandler_WhenBasketIsInactive_ThrowsDomainException()
    {
        await using var dbContext = CreateDbContext(nameof(RemoveBasketItemHandler_WhenBasketIsInactive_ThrowsDomainException));
        var productId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(productId, "Keyboard", 1, 25m);
        basket.Checkout();
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new RemoveBasketItemHandler(dbContext);

        var act = () => handler.ExecuteAsync(new RemoveBasketItemCommand(basket.Id, productId), CancellationToken.None);

        await act.Should().ThrowAsync<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_inactive" && exception.Field == "basketId")
            .WithMessage("Checked out baskets cannot be changed.");
    }

    [Fact]
    public async Task RemoveBasketItemHandler_WhenItemIsMissing_ThrowsDomainException()
    {
        await using var dbContext = CreateDbContext(nameof(RemoveBasketItemHandler_WhenItemIsMissing_ThrowsDomainException));
        var basket = Basket.Create(Guid.NewGuid());
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var handler = new RemoveBasketItemHandler(dbContext);

        var act = () => handler.ExecuteAsync(new RemoveBasketItemCommand(basket.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_item_not_found" && exception.Field == "productId")
            .WithMessage("The requested basket item was not found.");
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
