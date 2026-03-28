namespace Ecommerce.Basket.Domain;

public sealed class BasketItem
{
    private BasketItem()
    {
    }

    public BasketItem(string productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string ProductId { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

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
