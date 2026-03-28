namespace Ecommerce.BasketService.Domain;

public sealed class Basket : Entity
{
    private Basket()
    {
    }

    private Basket(Guid id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public string CustomerId { get; private set; } = string.Empty;

    public BasketStatus Status { get; private set; } = BasketStatus.Active;

    public decimal Total { get; private set; }

    public ICollection<BasketItem> Items { get; private set; } = new List<BasketItem>();

    public static Basket Create(Guid id, string customerId) => new(id, customerId);

    public bool IsActive => Status == BasketStatus.Active;

    public void AddOrUpdateItem(string productId, string productName, int quantity, decimal unitPrice)
    {
        var existingActiveItem = Items.SingleOrDefault(item => item.ProductId == productId && !item.IsDeleted);
        if (existingActiveItem is not null)
        {
            existingActiveItem.IncreaseQuantity(quantity);
            existingActiveItem.UpdateDetails(productName, unitPrice);
        }
        else
        {
            Items.Add(BasketItem.Create(Id, productId, productName, quantity, unitPrice));
        }

        RecalculateTotal();
    }

    public bool RemoveItem(string productId)
    {
        var item = Items.SingleOrDefault(existingItem => existingItem.ProductId == productId && !existingItem.IsDeleted);
        if (item is null)
        {
            return false;
        }

        Items.Remove(item);
        RecalculateTotal();
        return true;
    }

    public void Checkout()
    {
        Status = BasketStatus.CheckedOut;
        RaiseDomainEvent(new BasketCheckedOutDomainEvent(
            Id,
            CustomerId,
            Items
                .Where(item => !item.IsDeleted)
                .Select(item => new BasketCheckedOutItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                .ToArray(),
            Total));
    }

    private void RecalculateTotal()
    {
        Total = Items
            .Where(item => !item.IsDeleted)
            .Sum(item => item.Quantity * item.UnitPrice);
    }
}

