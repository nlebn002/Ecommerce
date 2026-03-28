namespace Ecommerce.BasketService.Domain;

public sealed class BasketItem : Entity
{
    private BasketItem()
    {
    }

    private BasketItem(Guid basketId, string productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        BasketId = basketId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid BasketId { get; private set; }

    public string ProductId { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static BasketItem Create(Guid basketId, string productId, string productName, int quantity, decimal unitPrice) =>
        new(basketId, productId, productName, quantity, unitPrice);

    public void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void UpdateDetails(string productName, decimal unitPrice)
    {
        ProductName = productName;
        UnitPrice = unitPrice;
    }

    public void Restore(int quantity, string productName, decimal unitPrice)
    {
        IsDeleted = false;
        Quantity = quantity;
        ProductName = productName;
        UnitPrice = unitPrice;
    }
}

