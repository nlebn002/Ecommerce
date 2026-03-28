namespace Ecommerce.BasketService.Domain;

public sealed record BasketCheckedOutDomainEvent(
    Guid BasketId,
    string CustomerId,
    IReadOnlyCollection<BasketCheckedOutItem> Items,
    decimal ItemsTotal) : IDomainEvent;

public sealed record BasketCheckedOutItem(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
