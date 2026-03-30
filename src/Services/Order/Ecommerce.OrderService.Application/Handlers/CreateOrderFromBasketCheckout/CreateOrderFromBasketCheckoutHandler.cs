using Ecommerce.OrderService.Domain;

namespace Ecommerce.OrderService.Application;

public sealed class CreateOrderFromBasketCheckoutHandler
{
    private readonly IOrderDbContext _dbContext;

    public CreateOrderFromBasketCheckoutHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto> ExecuteAsync(CreateOrderFromBasketCheckoutCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            command.CustomerId,
            command.Items
                .Select(item => new CreateOrderItemData(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                .ToArray(),
            command.ItemsTotal,
            command.CorrelationId,
            command.CausationId);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order.ToDetailsDto();
    }
}

public sealed record CreateOrderFromBasketCheckoutCommand(
    Guid CustomerId,
    IReadOnlyList<CreateOrderLineItem> Items,
    decimal ItemsTotal,
    Guid CorrelationId,
    Guid? CausationId);

public sealed record CreateOrderLineItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
