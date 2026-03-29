namespace Ecommerce.BasketService.Domain;

public sealed class BasketItem : Entity
{
    private BasketItem()
    {
    }

    private BasketItem(Guid basketId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureQuantityIsValid(quantity);
        EnsureUnitPriceIsValid(unitPrice);

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
        EnsureQuantityIsValid(quantity);
        Quantity += quantity;
    }

    public void AddQuantityAndUpdateDetails(string productName, int quantity, decimal unitPrice)
    {
        EnsureQuantityIsValid(quantity);
        EnsureUnitPriceIsValid(unitPrice);

        Quantity += quantity;
        ProductName = productName;
        UnitPrice = unitPrice;
    }

    public void UpdateDetails(string productName, decimal unitPrice)
    {
        EnsureUnitPriceIsValid(unitPrice);
        ProductName = productName;
        UnitPrice = unitPrice;
    }

    private static void EnsureQuantityIsValid(int quantity)
    {
        if (quantity < 1)
        {
            throw BasketException.Validation(BasketErrorCode.InvalidQuantity, "quantity", "Quantity must be greater than zero.");
        }
    }

    private static void EnsureUnitPriceIsValid(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw BasketException.Validation(BasketErrorCode.InvalidUnitPrice, "unitPrice", "Unit price must be greater than or equal to zero.");
        }
    }
}

