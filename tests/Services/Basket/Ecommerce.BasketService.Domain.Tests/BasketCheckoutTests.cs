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

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_empty" && exception.Field == "basketId")
            .WithMessage("The basket must contain at least one item before checkout.");
    }

    [Fact]
    public void Checkout_WhenBasketIsAlreadyCheckedOut_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, 20m);
        basket.Checkout();

        var act = basket.Checkout;

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_inactive" && exception.Field == "basketId")
            .WithMessage("The basket has already been checked out.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenQuantityIsInvalid_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 0, 20m);

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "invalid_quantity" && exception.Field == "quantity")
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenUnitPriceIsInvalid_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.AddOrUpdateItem(Guid.NewGuid(), "Keyboard", 1, -0.01m);

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "invalid_unit_price" && exception.Field == "unitPrice")
            .WithMessage("Unit price must be greater than or equal to zero.");
    }

    [Fact]
    public void AddOrUpdateItem_WhenExistingItemUpdateIsInvalid_DoesNotPartiallyMutateItem()
    {
        var productId = Guid.NewGuid();
        var basket = Basket.Create(Guid.NewGuid());
        basket.AddOrUpdateItem(productId, "Keyboard", 1, 20m);

        var act = () => basket.AddOrUpdateItem(productId, "Keyboard Pro", 2, -0.01m);

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "invalid_unit_price" && exception.Field == "unitPrice");

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

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_inactive" && exception.Field == "basketId")
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

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_inactive" && exception.Field == "basketId")
            .WithMessage("Checked out baskets cannot be changed.");
    }

    [Fact]
    public void RemoveItem_WhenItemIsMissing_ThrowsDomainException()
    {
        var basket = Basket.Create(Guid.NewGuid());

        var act = () => basket.RemoveItem(Guid.NewGuid());

        act.Should().Throw<BasketDomainException>()
            .Where(exception => exception.ErrorCode == "basket_item_not_found" && exception.Field == "productId")
            .WithMessage("The requested basket item was not found.");
    }
}
