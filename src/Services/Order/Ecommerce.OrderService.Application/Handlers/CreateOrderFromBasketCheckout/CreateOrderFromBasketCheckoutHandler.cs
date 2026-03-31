using Ecommerce.OrderService.Domain;
using Microsoft.EntityFrameworkCore;

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
        var existingOrder = await FindExistingOrderAsync(command.CorrelationId, cancellationToken);
        if (existingOrder is not null)
        {
            return existingOrder.ToDetailsDto();
        }

        var order = Order.Create(
            command.CustomerId,
            command.Items
                .Select(item => new CreateOrderItemData(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice))
                .ToArray(),
            command.ItemsTotal,
            command.CorrelationId,
            command.CausationId);

        _dbContext.Orders.Add(order);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            existingOrder = await FindExistingOrderAsync(command.CorrelationId, cancellationToken);
            if (existingOrder is not null)
            {
                return existingOrder.ToDetailsDto();
            }

            throw;
        }

        return order.ToDetailsDto();
    }

    private Task<Order?> FindExistingOrderAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.CheckoutCorrelationId == correlationId, cancellationToken);
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
