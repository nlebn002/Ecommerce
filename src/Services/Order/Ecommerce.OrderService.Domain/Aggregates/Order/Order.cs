namespace Ecommerce.OrderService.Domain;

public sealed class Order : Entity
{
    private Order()
    {
    }

    private Order(Guid id, Guid customerId, IReadOnlyCollection<OrderItem> items, decimal itemsTotal)
    {
        Id = id;
        CustomerId = customerId;
        Items = items.ToList();
        ItemsTotal = itemsTotal;
        FinalTotal = itemsTotal;
    }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public decimal ItemsTotal { get; private set; }

    public decimal ShippingPrice { get; private set; }

    public decimal FinalTotal { get; private set; }

    public string? CancellationReason { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    public static Order Create(
        Guid customerId,
        IReadOnlyCollection<CreateOrderItemData> items,
        decimal itemsTotal,
        Guid correlationId,
        Guid? causationId)
    {
        if (customerId == Guid.Empty)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidCustomerId, "customerId", "Customer id is required.");
        }

        if (items.Count == 0)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderItem, "items", "At least one order item is required.");
        }

        if (itemsTotal < 0)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderState, "itemsTotal", "Items total must be greater than or equal to zero.");
        }

        var orderId = Guid.NewGuid();
        var orderItems = items
            .Select(item => OrderItem.Create(orderId, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
            .ToArray();

        var order = new Order(orderId, customerId, orderItems, itemsTotal);
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(
            order.Id,
            order.CustomerId,
            order.ItemsTotal,
            order.Status.ToString(),
            correlationId,
            causationId));

        return order;
    }

    public bool IsPending => Status == OrderStatus.Pending;

    public void Confirm(decimal shippingPrice, Guid correlationId, Guid? causationId)
    {
        EnsurePending();

        if (shippingPrice < 0)
        {
            throw OrderException.Validation(OrderErrorCode.InvalidShippingPrice, "shippingPrice", "Shipping price must be greater than or equal to zero.");
        }

        ShippingPrice = shippingPrice;
        FinalTotal = ItemsTotal + shippingPrice;
        Status = OrderStatus.Confirmed;
        CancellationReason = null;

        RaiseDomainEvent(new OrderConfirmedDomainEvent(
            Id,
            ShippingPrice,
            FinalTotal,
            Status.ToString(),
            correlationId,
            causationId));
    }

    public void Cancel(string reason, Guid correlationId, Guid? causationId)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw OrderException.Validation(OrderErrorCode.InvalidOrderState, "reason", "Cancellation reason is required.");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        ShippingPrice = 0;
        FinalTotal = ItemsTotal;

        RaiseDomainEvent(new OrderCancelledDomainEvent(
            Id,
            reason,
            Status.ToString(),
            correlationId,
            causationId));
    }

    private void EnsurePending()
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Cancelled)
        {
            throw OrderException.Conflict(OrderErrorCode.ShipmentAlreadyProcessed, "Shipment has already been processed for this order.");
        }

        if (!IsPending)
        {
            throw OrderException.Conflict(OrderErrorCode.InvalidOrderState, "The order is not in a pending state.");
        }
    }
}

public sealed record CreateOrderItemData(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
