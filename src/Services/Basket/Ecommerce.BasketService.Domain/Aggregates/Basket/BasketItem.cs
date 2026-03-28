namespace Ecommerce.BasketService.Domain;

public sealed class BasketItem : Entity
{
    private BasketItem()
    {
    }

    private BasketItem(Guid basketId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        BasketId = basketId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid BasketId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static BasketItem Create(Guid basketId, Guid productId, string productName, int quantity, decimal unitPrice) =>
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
}

