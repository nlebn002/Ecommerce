namespace Ecommerce.BasketService.Domain;

public sealed record BasketCheckedOutDomainEvent(
    Guid BasketId,
    Guid CustomerId,
    IReadOnlyCollection<BasketCheckedOutItem> Items,
    decimal ItemsTotal) : IDomainEvent;

public sealed record BasketCheckedOutItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
