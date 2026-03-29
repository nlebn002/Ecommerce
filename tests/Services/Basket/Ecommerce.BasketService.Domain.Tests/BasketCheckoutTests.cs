using FluentAssertions;
using Xunit;

namespace Ecommerce.BasketService.Domain.Tests;

public sealed class BasketCheckoutTests
{
    [Fact]
    public void Checkout_SetsStatusToCheckedOut_AndRaisesCheckoutEvent()
    {
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 2, 25.50m);
        basket.AddOrUpdateItem(Guid.NewGuid(), "Mouse", 1, 10m);

        basket.Checkout();

        basket.Status.Should().Be(BasketStatus.CheckedOut);

        var domainEvent = basket.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BasketCheckedOutDomainEvent>()
            .Which;

        domainEvent.BasketId.Should().Be(basket.Id);
        domainEvent.CustomerId.Should().Be(basket.CustomerId);
        domainEvent.ItemsTotal.Should().Be(61m);
        domainEvent.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Checkout_UsesCurrentBasketSnapshot_ForPublishedItemPayload()
    {
        var firstProductId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(firstProductId, "Keyboard", 1, 20m);
        basket.AddOrUpdateItem(firstProductId, "Keyboard Pro", 2, 25m);

        basket.Checkout();

        var domainEvent = basket.DomainEvents.OfType<BasketCheckedOutDomainEvent>().Single();
        domainEvent.Items.Should().ContainSingle();

        var item = domainEvent.Items.Single();
        item.ProductId.Should().Be(firstProductId);
        item.ProductName.Should().Be("Keyboard Pro");
        item.Quantity.Should().Be(3);
        item.UnitPrice.Should().Be(25m);
        domainEvent.ItemsTotal.Should().Be(75m);
    }

    [Fact]
    public void Checkout_WhenBasketIsEmpty_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = basket.Checkout;

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.BasketEmpty && exception.Type == BasketErrorType.Validation)
            .WithMessage("The basket must contain at least one item before checkout.");
    }

    [Fact]
    public void Checkout_WhenBasketIsAlreadyCheckedOut_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, 20m);
        basket.Checkout();

        var act = basket.Checkout;

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.BasketInactive && exception.Type == BasketErrorType.Conflict)
            .WithMessage("The basket has already been checked out.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenQuantityIsInvalid_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 0, 20m);

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.InvalidQuantity && exception.Type == BasketErrorType.Validation)
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenUnitPriceIsInvalid_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, -0.01m);

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.InvalidUnitPrice && exception.Type == BasketErrorType.Validation)
            .WithMessage("Unit price must be greater than or equal to zero.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenExistingItemUpdateIsInvalid_DoesNotPartiallyMutateItem()
    {
        var productId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(productId, "Keyboard", 1, 20m);

        var act = () => basket.AddOrUpdateItem(productId, "Keyboard Pro", 2, -0.01m);

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.InvalidUnitPrice && exception.Type == BasketErrorType.Validation);

        basket.Total.Should().Be(20m);
        basket.Items.Should().ContainSingle();

        var item = basket.Items.Single();
        item.ProductName.Should().Be("Keyboard");
        item.Quantity.Should().Be(1);
        item.UnitPrice.Should().Be(20m);
    }

    [Fact]
    public void AddOrUpdateItem_WhenBasketIsInactive_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, 20m);
        basket.Checkout();

        var act = () => basket.AddOrUpdateItem(Guid.NewGuid(), "Mouse", 1, 10m);

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.BasketInactive && exception.Type == BasketErrorType.Conflict)
            .WithMessage("Checked out baskets cannot be changed.");
    }

    [Fact]
    public void RemoveItem_WhenBasketIsInactive_ThrowsDomainException()
    {
        var productId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(productId, "Keyboard", 1, 20m);
        basket.Checkout();

        var act = () => basket.RemoveItem(productId);

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.BasketInactive && exception.Type == BasketErrorType.Conflict)
            .WithMessage("Checked out baskets cannot be changed.");
    }

    [Fact]
    public void RemoveItem_WhenItemIsMissing_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.RemoveItem(Guid.NewGuid());

        act.Should().Throw<BasketException>()
            .Where(exception => exception.Code == BasketErrorCode.BasketItemNotFound && exception.Type == BasketErrorType.Validation)
            .WithMessage("The requested basket item was not found.");
    }

    [Fact]
    public void RemoveItem_SoftDeletesItem_AndRecalculatesTotal()
    {
        var removedProductId = Guid.NewGuid();
        var remainingProductId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(removedProductId, "Keyboard", 2, 25m);
        basket.AddOrUpdateItem(remainingProductId, "Mouse", 1, 10m);

        basket.RemoveItem(removedProductId);

        basket.Total.Should().Be(10m);
        basket.Items.Should().HaveCount(2);
        basket.Items.Should().Contain(item => item.ProductId == removedProductId && item.IsDeleted);
        basket.Items.Should().Contain(item => item.ProductId == remainingProductId && !item.IsDeleted);
    }

    [Fact]
    public void AddOrUpdateItem_WhenProductWasPreviouslyDeleted_CreatesNewActiveItem()
    {
        var productId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(productId, "Keyboard", 1, 20m);
        basket.RemoveItem(productId);

        basket.AddOrUpdateItem(productId, "Keyboard Pro", 2, 30m);

        basket.Total.Should().Be(60m);
        basket.Items.Should().HaveCount(2);
        basket.Items.Should().ContainSingle(item => item.ProductId == productId && item.IsDeleted);

        var activeItem = basket.Items.Should().ContainSingle(item => item.ProductId == productId && !item.IsDeleted).Subject;
        activeItem.ProductName.Should().Be("Keyboard Pro");
        activeItem.Quantity.Should().Be(2);
        activeItem.UnitPrice.Should().Be(30m);
    }
}
