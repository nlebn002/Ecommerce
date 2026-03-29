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
}
