namespace Ecommerce.BasketService.Domain;

public sealed class Basket
{
    private Basket()
    {
    }

    private Basket(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public string Id { get; private set; } = string.Empty;

    public string CustomerId { get; private set; } = string.Empty;

    public BasketStatus Status { get; private set; } = BasketStatus.Active;

    public decimal Total { get; private set; }

    public ICollection<BasketItem> Items { get; private set; } = new List<BasketItem>();

    public static Basket Create(string id, string customerId) => new(id, customerId);

    public bool IsActive => Status == BasketStatus.Active;

    public void AddOrUpdateItem(string productId, string productName, int quantity, decimal unitPrice)
    {
        var existingItem = Items.SingleOrDefault(item => item.ProductId == productId);
        if (existingItem is null)
        {
            Items.Add(new BasketItem(productId, productName, quantity, unitPrice));
        }
        else
        {
            existingItem.IncreaseQuantity(quantity);
            existingItem.UpdateDetails(productName, unitPrice);
        }

        RecalculateTotal();
    }

    public bool RemoveItem(string productId)
    {
        var item = Items.SingleOrDefault(existingItem => existingItem.ProductId == productId);
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
    }

    private void RecalculateTotal()
    {
        Total = Items.Sum(item => item.Quantity * item.UnitPrice);
    }
}

