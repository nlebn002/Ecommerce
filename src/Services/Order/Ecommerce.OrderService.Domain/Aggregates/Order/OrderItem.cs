namespace Ecommerce.OrderService.Domain;

public sealed class OrderItem : Entity
{
    private OrderItem()
    {
    }

    private OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static OrderItem Create(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderItem, "productId", "Product id is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderItem, "productName", "Product name is required.");
        }

        if (quantity <= 0)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderItem, "quantity", "Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderItem, "unitPrice", "Unit price must be greater than or equal to zero.");
        }

        return new OrderItem(orderId, productId, productName, quantity, unitPrice);
    }
}
